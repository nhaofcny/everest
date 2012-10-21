﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MohawkCollege.EHR.gpmr.Pipeline.Renderer.Java.Attributes;
using MohawkCollege.EHR.gpmr.Pipeline.Renderer.Java.Interfaces;
using MohawkCollege.EHR.gpmr.COR;
using System.IO;
using MohawkCollege.EHR.gpmr.Pipeline.Renderer.Java.HeuristicEngine;

namespace MohawkCollege.EHR.gpmr.Pipeline.Renderer.Java.Renderer
{
    [FeatureRenderer(Feature = typeof(MohawkCollege.EHR.gpmr.COR.ValueSet), IsFile = true)]
    [FeatureRenderer(Feature = typeof(MohawkCollege.EHR.gpmr.COR.CodeSystem), IsFile = true)]
    [FeatureRenderer(Feature = typeof(MohawkCollege.EHR.gpmr.COR.ConceptDomain), IsFile = true)]
    internal class EnumerationRenderer : IFeatureRenderer
    {
        /// <summary>
        /// Enumerations marked for use
        /// </summary>
        private static List<Enumeration> m_markedForUse = new List<Enumeration>(10);

        /// <summary>
        /// Mark as used
        /// </summary>
        /// <param name="enu"></param>
        /// <returns></returns>
        public static bool MarkAsUsed(Enumeration enu)
        {

            // This will check the literal count against bound vocab sets
            // if the enu is currently a concept domain
            if (enu is ConceptDomain && (enu as ConceptDomain).ContextBinding != null)
                enu = (enu as ConceptDomain).ContextBinding[0];

            if (!m_markedForUse.Contains(enu))
                m_markedForUse.Add(enu);
            return true;
        }

        /// <summary>
        /// Determine if we'll render this or not
        /// </summary>
        public static string WillRender(Enumeration enu)
        {
            if (!String.IsNullOrEmpty(Datatypes.GetBuiltinVocabulary(enu.Name)))
                return enu.Name;

            // This will check the literal count against bound vocab sets
            // if the enu is currently a concept domain
            if (enu is ConceptDomain && (enu as ConceptDomain).ContextBinding != null && (enu as ConceptDomain).ContextBinding.Count == 1)
                enu = (enu as ConceptDomain).ContextBinding[0];
            else if (enu is ConceptDomain && (enu as ConceptDomain).ContextBinding != null && (enu as ConceptDomain).ContextBinding.Count > 1) // HACK: If there is more than one context binding create a new value set, clear the binding and then re-bind
            {
                // Create the VS
                ValueSet vsNew = new ValueSet()
                {
                    Name = String.Format("{0}AutoGen", enu.Name),
                    BusinessName = enu.BusinessName,
                    Documentation = new Documentation()
                    {
                        Description = new List<string>(new string[] { 
                            String.Format("Value set has automatically been generated by GPMR to allow binding to ConceptDomain '{0}'", enu.Name)
                        }),
                        Rationale = new List<string>()
                    },
                    Id = enu.Id,
                    Literals = new List<Enumeration.EnumerationValue>(),
                    MemberOf = enu.MemberOf,
                    OwnerRealm = enu.OwnerRealm
                };

                // Add literals and documentation
                vsNew.Documentation.Rationale.Add(String.Format("GPMR can normally only redirect context bindings from a concept domain if only 1 is present, however this concept domain has '{0}' present. This value set is a union of content from:", (enu as ConceptDomain).ContextBinding.Count));
                foreach (Enumeration vs in (enu as ConceptDomain).ContextBinding)
                {

                    // If any of the context binding codes are not to be rendered do not render any of them
                    if (WillRender(vs) == String.Empty)
                        return String.Empty;

                    // Output rationale
                    vsNew.Documentation.Rationale.Add(String.Format("<p>- {0} ({1})</p>", vs.Name, vs.EnumerationType));
                    // Add literals
                    vsNew.Literals.AddRange(vs.GetEnumeratedLiterals());
                }

                // Now fire parse to add to the domain
                vsNew.FireParsed();

                // Replace the context bindings
                (enu as ConceptDomain).ContextBinding.Clear();
                (enu as ConceptDomain).ContextBinding.Add(vsNew);

                // redirect
                enu = vsNew;

            }
            else if (enu is ConceptDomain)
                return String.Empty;

            // Partial enumerations or suppressed enumerations are not to be included
            if (enu.IsPartial && !RimbaJavaRenderer.RenderPartials)
                return String.Empty;

            // Too big
            if (enu.GetEnumeratedLiterals().Count > RimbaJavaRenderer.MaxLiterals)
                return String.Empty;

            // Already has a preferred name?
            if (enu.Annotations != null && enu.Annotations.Exists(o => o is RenderAsAnnotation))
                return (enu.Annotations.Find(o => o is RenderAsAnnotation) as RenderAsAnnotation).RenderName;

            // Already being used
            if (m_markedForUse.Exists(o => o.Name == enu.Name && o.GetType() == enu.GetType()))
                return enu.Name;

            string name = enu.Name;

            if (enu.GetEnumeratedLiterals().Count > 0 && enu.GetEnumeratedLiterals().FindAll(l => !l.Annotations.Exists(o => o is SuppressBrowseAnnotation)).Count > 0 &&
                (RimbaJavaRenderer.GenerateVocab || (!RimbaJavaRenderer.GenerateVocab && enu is ValueSet)))
            {
                // Name collision? Resolve
                if (enu.MemberOf.Find(o => o.Name == enu.Name && o.GetType() != enu.GetType() &&
                    !(o is ConceptDomain)) != null)
                {
                    if (m_markedForUse.Exists(o => o.Name == enu.Name && o.GetType() != enu.GetType()))
                    {
                        name = String.Format("{0}1", enu.Name);
                        if (enu.Annotations == null)
                            enu.Annotations = new List<Annotation>();
                        enu.Annotations.Add(new RenderAsAnnotation() { RenderName = name });
                    }
                }
                return name; // don't process
            }
            return String.Empty;
        }

        #region IFeatureRenderer Members

        /// <summary>
        /// Render the enumeration
        /// </summary>
        public void Render(string ownerPackage, string apiNs, Feature f, System.IO.TextWriter tw)
        {
            // Validate arguments
            if (String.IsNullOrEmpty(ownerPackage))
                throw new ArgumentNullException("ownerPackage");
            if (String.IsNullOrEmpty(apiNs))
                throw new ArgumentNullException("apiNs");
            if (f == null || !(f is Enumeration))
                throw new ArgumentException("Parameter must be of type Enumeration", "f");

            Enumeration cls = f as Enumeration;

            // enumeration is a concept domain? do the binding
            if (cls is ConceptDomain && (cls as ConceptDomain).ContextBinding != null)
                cls = (cls as ConceptDomain).ContextBinding[0];
            else if (cls is ConceptDomain)
                throw new InvalidOperationException("Won't render unbound concept domains");


            tw.WriteLine("package {0}.vocabulary;", ownerPackage);

            #region Render the imports
            string[] apiImports = { "annotations.*", "datatypes.*", "datatypes.generic.*" },
                jImports = { "java.lang.*", "java.util.*" };
            foreach (var import in apiImports)
                tw.WriteLine("import {0}.{1};", apiNs, import);
            foreach (var import in jImports)
                tw.WriteLine("import {0};", import);
            #endregion

            #region Render Class Signature

            // Documentation
            if (DocumentationRenderer.Render(cls.Documentation, 0).Length == 0)
                tw.WriteLine("/** No Summary Documentation Found */");
            else
                tw.Write(DocumentationRenderer.Render(cls.Documentation, 0));

            // Create structure annotation
            tw.WriteLine(CreateStructureAnnotation(cls));

            string renderName = cls.Name;
            if (cls.Annotations != null && cls.Annotations.Exists(o => o is RenderAsAnnotation))
                renderName = (cls.Annotations.Find(o => o is RenderAsAnnotation) as RenderAsAnnotation).RenderName;

            // Create class signature
            tw.Write("public class {0} implements {1}.interfaces.IEnumeratedVocabulary", Util.Util.MakeFriendly(renderName), apiNs);

            tw.WriteLine("{");

            #endregion

            #region Render Properties

            StringWriter sw = new StringWriter();
            RenderLiterals(sw, cls, new List<string>(), new List<string>(), cls.Literals, renderName);
            String tStr = sw.ToString();
            tw.WriteLine(tStr);

            #endregion

            #region Render IEnumeratedVocabulary Methods

            tw.WriteLine("\tpublic {0}(String code, String codeSystem) {{ this.m_code = code; this.m_codeSystem = codeSystem; }}", Util.Util.MakeFriendly(renderName));
            tw.WriteLine("\tprivate final String m_code;");
            tw.WriteLine("\tprivate final String m_codeSystem;");
            tw.WriteLine("\tpublic String getCodeSystem() { return this.m_codeSystem; }");
            tw.WriteLine("\tpublic String getCode() { return this.m_code; }");
	
            #endregion
            // End enumeration
            tw.WriteLine("}");
        }

        /// <summary>
        /// Create structure attribute
        /// </summary>
        private string CreateStructureAnnotation(Enumeration cls)
        {
            return String.Format("@Structure(name = \"{0}\", codeSystem = \"{1}\", structureType = StructureType.{2})", cls.Name, cls.ContentOid, cls.GetType().Name.ToUpper());
        }

        /// <summary>
        /// Render literals
        /// </summary>
        private void RenderLiterals(StringWriter sw, Enumeration enu, List<string> rendered, List<String> mnemonics, List<Enumeration.EnumerationValue> literals, string ctorName)
        {
            // Literals
            foreach (Enumeration.EnumerationValue ev in literals)
            {
                string bn = Util.Util.PascalCase(ev.BusinessName);
                string rendName = Util.Util.PascalCase(bn ?? ev.Name) ?? "__Unknown";

                // Already rendered, so warn and skip
                if (rendered.Contains(rendName) || mnemonics.Contains(ev.Name))
                {
                    System.Diagnostics.Trace.WriteLine(String.Format("Enumeration value {0} already rendered, skipping", ev.BusinessName), "warn");
                    continue;
                }
                else if (!ev.Annotations.Exists(o => o is SuppressBrowseAnnotation))
                {
                    sw.Write(DocumentationRenderer.Render(ev.Documentation, 1));
                    if (DocumentationRenderer.Render(ev.Documentation, 1).Length == 0) // Documentation correction
                        sw.WriteLine("\t/** {0} */", (ev.BusinessName ?? ev.Name).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r", "").Replace("\n", ""));

                    // Annotations?
                    if (ev.Annotations != null && ev.Annotations.Find(o => o is SuppressBrowseAnnotation) != null)
                    {
                        // Can't suppress browse in Jaba
                        System.Diagnostics.Trace.WriteLine(String.Format("Enumation literal '{0}' won't be rendered as it has SuppressBrowse enabled", ev.Name));
                        //sw.WriteLine("\t\t[EditorBrowsable(EditorBrowsableState.Never)]\r\n\t\t[Browsable(false)]");
                    }

                    // Render ?
                    if (rendered.Find(o => o.Equals(rendName)) != null) // .NET enumeration field will be the same, so render something different
                        sw.Write("\tpublic static final {3} {0} = new {3}(\"{1}\",\"{2}\")", Util.Util.MakeFriendly(rendName + "_" + ev.Name) ?? "__Unknown",
                            ev.Name, ev.CodeSystem ?? enu.ContentOid, Util.Util.MakeFriendly(ctorName));
                    else
                        sw.Write("\tpublic static final {3} {0} = new {3}(\"{1}\",\"{2}\")", rendName, ev.Name, ev.CodeSystem ?? enu.ContentOid, Util.Util.MakeFriendly(ctorName));

                    sw.WriteLine(";"); // Another literal follows

                    sw.Write("\r\n"); // Newline

                    rendered.Add(rendName); // Add to rendered list to keep track
                    mnemonics.Add(ev.Name);

                    if (ev.RelatedCodes != null)
                        RenderLiterals(sw, enu, rendered, mnemonics, ev.RelatedCodes, ctorName);
                }
            }
        }

        /// <summary>
        /// Create a file for the enumeration
        /// </summary>
        public string CreateFile(Feature f, string filePath)
        {

            string fileName = Util.Util.MakeFriendly(f.Name);

            // Render as
            if (f.Annotations != null && f.Annotations.Exists(o => o is RenderAsAnnotation))
                fileName = Util.Util.MakeFriendly((f.Annotations.Find(o => o is RenderAsAnnotation) as RenderAsAnnotation).RenderName);

            fileName = Path.ChangeExtension(Path.Combine(Path.Combine(filePath, "vocabulary"), Util.Util.MakeFriendly(fileName)), ".java");


            var enu = f as Enumeration;

            if (!String.IsNullOrEmpty(Datatypes.GetBuiltinVocabulary(enu.Name)))
                throw new InvalidOperationException("Enumeration is builtin to core library. Will not render");

            // Is this code system even used?
            if (!m_markedForUse.Exists(o => o.GetType().Equals(f.GetType()) && o.Name == f.Name))
            {
                if (enu.GetEnumeratedLiterals().Count > RimbaJavaRenderer.MaxLiterals)
                    throw new InvalidOperationException(String.Format("Enumeration '{2}' too large, enumeration has {0} literals, maximum allowed is {1}",
                        enu.GetEnumeratedLiterals().Count, RimbaJavaRenderer.MaxLiterals, enu.Name));
                else
                    throw new InvalidOperationException("Enumeration is not used, or is an unbound concept domain!");
            }



            // First, if the feature is a value set we will always render it
            if (File.Exists(fileName) && !(f as Enumeration).OwnerRealm.EndsWith(RimbaJavaRenderer.prefRealm))
                throw new InvalidOperationException("Enumeration has already been rendered from the preferred realm. Will not render this feature");

            return fileName;

        }

        #endregion
    }
}
