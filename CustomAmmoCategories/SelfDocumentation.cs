using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustAmmoCategories {
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SkipDocumentationAttribute : System.Attribute {
    public SkipDocumentationAttribute() { }
  }
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SelfDocumentationTypeName : System.Attribute {
    public string typeName { get; private set; }
    public SelfDocumentationTypeName(string typeName) { this.typeName = typeName; }
  }
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SelfDocumentationName : System.Attribute {
    public string Name { get; private set; }
    public SelfDocumentationName(string n) { this.Name = n; }
  }
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SelfDocumentationDefaultValue : System.Attribute {
    public string defValue { get; private set; }
    public SelfDocumentationDefaultValue(string def) { this.defValue = def; }
  }
  [System.AttributeUsage(System.AttributeTargets.Class)]
  public class SelfDocumentedClass : System.Attribute {
    public string name { get; private set; }
    public string node { get; private set; }
    public string directory { get; private set; }
    public SelfDocumentedClass(string dir,string node, string name) { this.node = node; this.directory = dir; this.name = name; }
  }
  public class SelfDocumentationRecord {
    public string id { get; set; }
    public string description { get; set; }
    public Dictionary<string, string> defaultValue { get; set; } = new Dictionary<string, string>();
    public string typeName { get; set; }
    public HashSet<string> classes { get; set; } = new HashSet<string>();
    public void setDefaultValues(string[] values) {
      foreach (string val in values) {
        string[] val_key = val.Split('=');
        if (val_key.Length < 2) { continue; }
        defaultValue.Add(val_key[0],val_key[1]);
      }
    }
    public void setDefaultValue(string className, object defVal) {
      string dv = string.Empty;
      if (defVal == null) {
        dv = "undefined";
      } else
      if (defVal.GetType().IsClass) {
        dv = "empty";
      } else {
        if (SelfDocumentationHelper.isLowerValue.TryGetValue(defVal.GetType(), out bool is_lower) == false) {
          is_lower = false;
        }
        dv = is_lower ? defVal.ToString().ToLower() : defVal.ToString();
      }
      setDefaultValue(className, dv);
    }
    public void setDefaultValue(string className, string defVal) {
      if (this.defaultValue.ContainsKey(className)) {
        this.defaultValue[className] = defVal;
      } else {
        this.defaultValue.Add(className,defVal);
      }
    }
  }
  public class SelfDocumentationNode {
    public Dictionary<string, SelfDocumentationRecord> docProperties { get; set; } = new Dictionary<string, SelfDocumentationRecord>();
    public void read(string docFile) {
      string[] content = File.ReadAllLines(docFile);
      SelfDocumentationRecord docRec = null;
      bool description_started = false;
      foreach (string line in content) {
        if (line.StartsWith("@Name:")) {
          if (docRec != null) {
            //docRec.descrSanitise();
            if (docProperties.ContainsKey(docRec.id) == false) { docProperties.Add(docRec.id, docRec); } else { docProperties[docRec.id] = docRec; };
          }
          docRec = new SelfDocumentationRecord();
          docRec.id = line.Substring("@Name:".Length);
          description_started = false;
        }
        if (line.StartsWith("@Default:")) {
          if (docRec != null) {
            docRec.setDefaultValues(line.Substring("@Default:".Length).Split(';'));
          }
          description_started = false;
        } else
        if (line.StartsWith("@Type:")) {
          if (docRec != null) {
            docRec.typeName = line.Substring("@Type:".Length);
          }
          description_started = false;
        } else
        if (line.StartsWith("@AppliedTo:")) {
          if (docRec != null) {
            docRec.classes = line.Substring("@AppliedTo:".Length).Split(',').ToHashSet();
          }
          description_started = false;
        } else
        if (line.StartsWith("@Description:")) {
          if (docRec != null) {
            docRec.description = line.Substring("@Description:".Length);
          }
          description_started = true;
        } else
        if (description_started) {
          if (docRec != null) {
            docRec.description += (line + "\n");
          }
        }
      }
      if (docRec != null) {
        //docRec.descrSanitise();
        if (docProperties.ContainsKey(docRec.id) == false) { docProperties.Add(docRec.id, docRec); } else { docProperties[docRec.id] = docRec; };
      }
    }
  }
  public class SelfDocumentationDirectory {
    public Dictionary<string, SelfDocumentationNode> nodes { get; set; } = new Dictionary<string, SelfDocumentationNode>();
    public void read(string dir) {
      string[] docFiles = Directory.GetFiles(dir, "properties_*.txt", SearchOption.TopDirectoryOnly);
      foreach(string docFile in docFiles) {
        string nodeName = Path.GetFileNameWithoutExtension(docFile).Substring("properties_".Length);
        if (nodes.TryGetValue(nodeName,out SelfDocumentationNode docNode) == false) {
          docNode = new SelfDocumentationNode();
          nodes.Add(nodeName,docNode);
        }
        docNode.read(docFile);
      }
    }
  }
  public static class SelfDocumentationHelper {
    public static Dictionary<Type, bool> isLowerValue = new Dictionary<Type, bool>() { { typeof(bool), true } };
    public static Dictionary<Type, string> typeDocNames = new Dictionary<Type, string>() {
      { typeof(float), "float" }
      , { typeof(int), "integer" }
      , { typeof(TripleBoolean), "boolean" }
      , { typeof(bool), "boolean" }
      , { typeof(string), "string" }
    };
    public static void set_defaultValue(this SelfDocumentationRecord docRec, object defVal) {
    }
    public static void set_typeName(this SelfDocumentationRecord docRec, Type valType) {
      if (typeDocNames.TryGetValue(valType, out string typeDocName)) {
        docRec.typeName = typeDocName;
        return;
      }
      if (valType.IsEnum) {
        docRec.typeName = "enum. Possible values: ";
        bool first = true;
        foreach (object val in Enum.GetValues(valType)) {
          if (first) { first = false; } else { docRec.typeName += ", "; };
          docRec.typeName += val.ToString();
        }
        return;
      }
      docRec.typeName = valType.Name;
    }
    public static void CreateSelfDocumentation(Type selfDocumentatedType, Dictionary<string, SelfDocumentationDirectory> full_documentation) {
      object selfDocumentedInstance = Activator.CreateInstance(selfDocumentatedType);
      string docNodeName = selfDocumentatedType.GetSelfDocumentationNode();
      string docDirectory = selfDocumentatedType.GetSelfDocumentationDirectory();
      string className = selfDocumentatedType.GetSelfDocumentationName();
      Log.M.TWL(0, "self documentation: "+docDirectory+"/"+ docNodeName + "."+className);
      PropertyInfo[] props = selfDocumentatedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      //string docFile = Path.Combine(directory, "Readme_" + selfDocumentedInstance.SelfDocumentationName + "_documentation.txt");
      //Dictionary<string, SelfDocumantationRecord> documntation = new Dictionary<string, SelfDocumantationRecord>();
      if (full_documentation.TryGetValue(docDirectory, out SelfDocumentationDirectory documentation) == false) {
        documentation = new SelfDocumentationDirectory();
        full_documentation.Add(docDirectory, documentation);
      }
      if (documentation.nodes.TryGetValue(docNodeName, out SelfDocumentationNode docNode) == false) {
        docNode = new SelfDocumentationNode();
        documentation.nodes.Add(docNodeName, docNode);
      }
      //ReadDocumentation(docFile, documntation);
      foreach (PropertyInfo prop in props) {
        object[] attrs = prop.GetCustomAttributes(true);
        bool skipDocumentation = false;
        foreach (object attr in attrs) { if (attr as SkipDocumentationAttribute != null) { skipDocumentation = true; break; } }
        if (skipDocumentation) { continue; }
        if(docNode.docProperties.TryGetValue(prop.Name, out SelfDocumentationRecord docRec) == false) {
          docRec = new SelfDocumentationRecord();
          docRec.id = prop.Name;
          docNode.docProperties.Add(docRec.id, docRec);
        }
        bool isStatValue = false;
        foreach (object attr in attrs) {
          StatCollectionFloatAttribute statAttr = attr as StatCollectionFloatAttribute;
          if (statAttr != null) { isStatValue = true; break; }
        }
        if (string.IsNullOrEmpty(docRec.description)) { docRec.description = "not documented yet\n"; }
        if (isStatValue) {
          string statistic = "Can be accessed via statistic value " + ExtWeaponDef.StatisticAttributePrefix + docRec.id + " and modifier " + ExtWeaponDef.StatisticAttributePrefix + docRec.id + ExtWeaponDef.StatisticModifierSuffix;
          if(docRec.description.Contains(statistic) == false) {
            if (docRec.description.EndsWith("\n") == false) {
              docRec.description += "\n";
            }
            docRec.description += (statistic+"\n");
          }
        }
        if (docRec.classes.Contains(className) == false) { docRec.classes.Add(className); }
        SelfDocumentationDefaultValue defVal = null;
        foreach (object attr in attrs) {
          defVal = attr as SelfDocumentationDefaultValue;
          if (defVal != null) { break; }
        }
        if(defVal != null) {
          docRec.setDefaultValue(className, defVal.defValue);
        } else {
          docRec.setDefaultValue(className, prop.GetValue(selfDocumentedInstance));
        }
        SelfDocumentationTypeName typeName = null;
        foreach (object attr in attrs) {
          typeName = attr as SelfDocumentationTypeName;
          if (typeName != null) { break; }
        }
        if (typeName != null) {
          docRec.typeName = typeName.typeName;
        } else {
          docRec.set_typeName(prop.PropertyType);
        }
      }
    }
    public static bool isSelfDocumented(this Type t) {
      foreach (var attr in t.GetCustomAttributes(true)) {
        if (attr as SelfDocumentedClass != null) { return true; }
      }
      return false;
    }
    public static string GetSelfDocumentationNode(this Type t) {
      foreach (var attr in t.GetCustomAttributes(true)) {
        SelfDocumentedClass sd_attr = attr as SelfDocumentedClass;
        if (sd_attr != null) { return sd_attr.node; }
      }
      return string.Empty;
    }
    public static string GetSelfDocumentationDirectory(this Type t) {
      foreach (var attr in t.GetCustomAttributes(true)) {
        SelfDocumentedClass sd_attr = attr as SelfDocumentedClass;
        if (sd_attr != null) { return sd_attr.directory; }
      }
      return string.Empty;
    }
    public static string GetSelfDocumentationName(this Type t) {
      foreach (var attr in t.GetCustomAttributes(true)) {
        SelfDocumentedClass sd_attr = attr as SelfDocumentedClass;
        if (sd_attr != null) { return sd_attr.name; }
      }
      return string.Empty;
    }
    public static string GetSelfDocumentationDefaultVal(this PropertyInfo prop, object instance) {
      foreach (var attr in prop.GetCustomAttributes(true)) {
        SelfDocumentationDefaultValue doc_attr = attr as SelfDocumentationDefaultValue;
        if (doc_attr != null) { return doc_attr.defValue; }
      }
      object val = prop.GetValue(instance);
      if (val == null) { return "undefined"; }
      if (val.GetType() == typeof(TripleBoolean)) { if ((TripleBoolean)val == TripleBoolean.NotSet) { return "undefined"; } }
      if (val.GetType() == typeof(string)) { if (string.IsNullOrEmpty((string)val)) { return "undefined"; } }
      if (isLowerValue.TryGetValue(val.GetType(), out bool is_lower) == false) {
        is_lower = false;
      }
      return is_lower ? val.ToString().ToLower() : val.ToString();
    }
    public static void CreateSelfDocumentation(string directory) {
      var selfDocTypes = Assembly.GetExecutingAssembly().GetTypes().Where(p => p.isSelfDocumented());
      string documentationDir = Path.Combine(directory, "documentation");
      if (Directory.Exists(documentationDir) == false) { Directory.CreateDirectory(documentationDir); };
      string[] docDirs = Directory.GetDirectories(documentationDir, "*", SearchOption.TopDirectoryOnly);
      Dictionary<string, SelfDocumentationDirectory> documentation = new Dictionary<string, SelfDocumentationDirectory>();
      foreach (string docDir in docDirs) {
        if(documentation.TryGetValue(docDir, out SelfDocumentationDirectory selfDocDir) == false) {
          selfDocDir = new SelfDocumentationDirectory();
          documentation.Add(docDir, selfDocDir);
        }
        selfDocDir.read(docDir);
      }
      //Dictionary<string, Dictionary<string, SelfDocumentationRecord>> documentation = new Dictionary<string, Dictionary<string, SelfDocumentationRecord>>();
      foreach (var selfDocType in selfDocTypes) {
        CreateSelfDocumentation(selfDocType, documentation);
      }
      foreach(var nodeDocs in documentation) {
        string nodeDir = Path.Combine(documentationDir, nodeDocs.Key);
        if (Directory.Exists(nodeDir) == false) { Directory.CreateDirectory(nodeDir); };
        foreach(var nodeDoc in nodeDocs.Value.nodes) {
          string fileName = Path.Combine(nodeDir,"properties_"+nodeDoc.Key+".txt");
          StringBuilder content = new StringBuilder();
          List<KeyValuePair<string, SelfDocumentationRecord>> records = nodeDoc.Value.docProperties.ToList();
          records.Sort((x,y)=> { return x.Key.CompareTo(y.Key); });
          foreach (var docRecord in records) {
            content.AppendLine("@Name:"+ docRecord.Value.id);
            content.AppendLine("@Type:" + docRecord.Value.typeName);
            content.Append("@AppliedTo:");
            {
              bool flag = true;
              foreach (string className in docRecord.Value.classes) {
                if (flag) { flag = false; } else { content.Append(","); }; content.Append(className);
              };
            };
            content.AppendLine();
            content.Append("@Default:");
            {
              bool flag = true;
              foreach (var defVal in docRecord.Value.defaultValue) {
                if (flag) { flag = false; } else { content.Append(","); }; content.Append(defVal.Key+"="+defVal.Value);
              };
            };
            content.AppendLine();
            content.AppendLine("@Description:" + docRecord.Value.description);
          }
          File.WriteAllText(fileName, content.ToString());
        }
      }
    }
  }
}