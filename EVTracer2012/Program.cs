using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Viewer;

namespace EVTracer2012 {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            OpenFileDialog ofdxml = new OpenFileDialog();
            ofdxml.Filter = "*.xml|*.xml";
            SaveFileDialog sfdxml = new SaveFileDialog();
            sfdxml.Filter = "*.xml|*.xml";

            SaveFileDialog sfdcsv = new SaveFileDialog();
            sfdcsv.Filter = "*.csv|*.csv";

            if (args.Length == 1 && String.Compare(args[0], "/format", true) == 0) {
                if (ofdxml.ShowDialog() == DialogResult.OK) {
                    XDocument xo = XDocument.Load(ofdxml.FileName);
                    sfdxml.FileName = ofdxml.FileName;
                    if (sfdxml.ShowDialog() == DialogResult.OK) {
                        xo.Save(sfdxml.FileName);
                    }
                }
            }
            bool conv = false, convf = false;
            if (args.Length == 1 && ((convf = String.Compare(args[0], "/convf", true) == 0) || (conv = String.Compare(args[0], "/conv", true) == 0))) {
                if (ofdxml.ShowDialog() == DialogResult.OK) {
                    XDocument xo = XDocument.Load(ofdxml.FileName);
                    if (sfdcsv.ShowDialog() == DialogResult.OK) {
                        using (var wr = new StreamWriter(sfdcsv.FileName, false, Encoding.UTF8)) {
                            Csvw w = new Csvw(wr, ',', '"');
                            var evs = EVUt.Events(xo).ToArray();
                            var keys = evs.SelectMany(q => q.Keys).Distinct().ToArray();
                            foreach (var k in keys) {
                                w.Write(k);
                            }
                            w.NextRow();
                            String v;
                            foreach (var ev in evs.Where(q => conv || VUt.Gets(q, "EventID").Equals("4659"))) {
                                foreach (var k in keys) {
                                    w.Write(ev.TryGetValue(k, out v) ? v : "");
                                }
                                w.NextRow();
                            }
                        }
                    }
                }
            }
        }

        class EVUt {
            internal static IEnumerable<IDictionary<String, String>> Events(XDocument xo) {
                XAttribute att0 = new XAttribute("DUMMY", "");
                var ns = XNamespace.Get("http://schemas.microsoft.com/win/2004/08/events/event");
                foreach (XElement ev in xo.Elements("Events").Elements(ns + "Event")) {
                    var d = new Dictionary<string, string>();
                    var elsys = ev.Element(ns + "System");
                    if (elsys != null) {
                        foreach (var el1 in elsys.Elements()) {
                            d[el1.Name.LocalName] = el1.Value;

                            foreach (var at1 in el1.Attributes()) {
                                d[el1.Name.LocalName + "-" + at1.Name.LocalName] = at1.Value;
                            }
                        }
                    }
                    var eled = ev.Element(ns + "EventData");
                    if (eled != null) {
                        foreach (var el1 in eled.Elements()) {
                            d["EventData:" + el1.Attribute("Name").Value] = el1.Value;
                        }
                    }
                    var elri = ev.Element(ns + "RenderingInfo");
                    if (elri != null) {
                        String culture = (elri.Attribute("Culture") ?? att0).Value;

                        foreach (var el1 in elri.Elements()) {
                            d["RenderingInfo-" + culture + "-" + el1.Name.LocalName] = el1.Value;

                            foreach (var at1 in el1.Attributes()) {
                                d["RenderingInfo-" + culture + "-" + el1.Name.LocalName + "-" + at1.Name.LocalName] = at1.Value;
                            }
                        }
                    }
                    yield return d;
                }
            }
        }

        class VUt {
            internal static string Gets(IDictionary<string, string> d, String k) {
                String v;
                if (!d.TryGetValue(k, out v)) v = "";
                return v;
            }
        }
    }
}
