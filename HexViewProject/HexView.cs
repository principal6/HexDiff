using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using TreeStructure;

namespace HexViewProject
{
    using Tree      = Tree<NodeData>;
    using TreeNode  = Tree<NodeData>.Node;

    public enum BuiltInDataType
    {
        Wrapper,
        Int8,
        Uint8,
        Int16,
        Uint16,
        Int32,
        Uint32,
        Int64,
        Uint64,
        Float, // 32bit floating-point value
        Double, // 64bit (double-precision) floating-point value
        AnsiChar8, // char - CodePage 는 Document 세팅이 기본값, 각 Unit에서 오버라이딩 가능
        UTF8,
        UTF8BOM,
        UTF16 // wchar_t
    }

    public class HexViewDataType
    {
        public string                   Name;
        public string                   Alias;
        public string                   Content;
        public string                   Format;
        public int                      ByteCount;
        public HexViewDataType          Parent      = null;
        public List<HexViewDataType>    Children;

        public HexViewDataType()
        {

        }

        public HexViewDataType(BuiltInDataType builtInDataType)
        {
            Name = builtInDataType.ToString();
            ByteCount = HexViewDataType.getBuiltInTypeByteCount(builtInDataType);
        }

        static public BuiltInDataType stringToBuiltInDataType(string typeName)
        {
            if (typeName == "Int8")
            {
                return BuiltInDataType.Int8;
            }
            else if (typeName == "Uint8")
            {
                return BuiltInDataType.Uint8;
            }
            else if (typeName == "Int16")
            {
                return BuiltInDataType.Int16;
            }
            else if (typeName == "Uint16")
            {
                return BuiltInDataType.Uint16;
            }
            else if (typeName == "Int32")
            {
                return BuiltInDataType.Int32;
            }
            else if (typeName == "Uint32")
            {
                return BuiltInDataType.Uint32;
            }
            else if (typeName == "Int64")
            {
                return BuiltInDataType.Int64;
            }
            else if (typeName == "Uint64")
            {
                return BuiltInDataType.Uint64;
            }
            else if (typeName == "Float")
            {
                return BuiltInDataType.Float;
            }
            else if (typeName == "Double")
            {
                return BuiltInDataType.Double;
            }
            else if (typeName == "AnsiChar8")
            {
                return BuiltInDataType.AnsiChar8;
            }
            else if (typeName == "UTF8")
            {
                return BuiltInDataType.UTF8;
            }
            else if (typeName == "UTF16")
            {
                return BuiltInDataType.UTF16;
            }
            return BuiltInDataType.Wrapper;
        }

        static public int getBuiltInTypeByteCount(BuiltInDataType builtInType)
        {
            if (builtInType == BuiltInDataType.AnsiChar8 ||
                builtInType == BuiltInDataType.Int8 ||
                builtInType == BuiltInDataType.Uint8)
            {
                return 1;
            }
            else if (builtInType == BuiltInDataType.Int16 ||
                builtInType == BuiltInDataType.Uint16)
            {
                return 2;
            }
            else if (builtInType == BuiltInDataType.Int32 ||
                builtInType == BuiltInDataType.Uint32 ||
                builtInType == BuiltInDataType.Float)
            {
                return 4;
            }
            else if (builtInType == BuiltInDataType.Int64 ||
                builtInType == BuiltInDataType.Uint64 ||
                builtInType == BuiltInDataType.Double)
            {
                return 8;
            }

            return -1; // 가변 길이!!
        }

        public bool isBuiltInDataType()
        {
            BuiltInDataType result = stringToBuiltInDataType(Name);
            return (result != BuiltInDataType.Wrapper);
        }
    }

    public class NodeData
    {
        public NodeData()
        {
        }

        public NodeData(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            System.Type type = obj.GetType();
            if (type.Name == "String")
            {
                return (Name == (obj as string));
            }
            if (GetType() == obj.GetType())
            {
                return (Name == (obj as NodeData).Name);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string           Name;
        public int              Count   = 1;
        public HexViewDataType  Type;
        public string           Tip;
    }

    public class HexView
    {
        public List<HexViewDataType>    _typeDefinitions;
        public Tree                     _doc;
        public int                      _defaultCodePage;

        HexViewDataType findDataType(string key)
        {
            return _typeDefinitions.Find(
                delegate (HexViewDataType a)
                {
                    return (a.Alias == key || a.Name == key);
                });
        }

        private bool constructDataTypeInternal(ref HexViewDataType currDataType, XmlNode xmlNode)
        {
            XmlNode attrName = xmlNode.Attributes.GetNamedItem("Name");
            if (attrName != null)
            {
                currDataType.Alias = attrName.Value;
            }

            XmlNode attrType = xmlNode.Attributes.GetNamedItem("Type");
            if (attrType != null)
            {
                BuiltInDataType builtInType = HexViewDataType.stringToBuiltInDataType(attrType.Value);
                if (builtInType == BuiltInDataType.Wrapper)
                {
                    HexViewDataType found = findDataType(attrType.Value);
                    if (found == null)
                    {
                        MessageBox.Show("해당 데이터 타입이 선언되지 않았습니다. 선언 순서를 지켰습니까?", "사용자 정의 자료형 파싱 실패");
                        return false;
                    }
                    else
                    {
                        currDataType.Name = found.Name;
                        currDataType.Children = found.Children;
                        currDataType.ByteCount = found.ByteCount;
                    }
                }
                else
                {
                    currDataType.Name = builtInType.ToString();
                    currDataType.ByteCount = HexViewDataType.getBuiltInTypeByteCount(builtInType);
                }
            }

            // 자식이 있는 경우 자식의 ByteCount 를 누적한 게 내 ByteCount 가 된다!
            if (currDataType.Children != null)
            {
                foreach (HexViewDataType childNode in currDataType.Children)
                {
                    currDataType.ByteCount += childNode.ByteCount;
                }
            }

            XmlNode attrContent = xmlNode.Attributes.GetNamedItem("Content");
            if (attrContent != null)
            {
                currDataType.Content = attrContent.Value;
            }

            XmlNode attrFormat = xmlNode.Attributes.GetNamedItem("Format");
            if (attrFormat != null)
            {
                currDataType.Format = attrFormat.Value;
            }

            return true;
        }

        private void constructDataTypeArray(XmlNode xmlNode, HexViewDataType parentDataType)
        {
            for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
            {
                XmlNode nodeType = xmlNode.ChildNodes[i];

                XmlNode attrName = nodeType.Attributes.GetNamedItem("Name");
                if (attrName == null)
                {
                    MessageBox.Show("Name 속성이 없는 노드입니다. xml 파일을 확인해 주세요", "사용자 정의 자료형 파싱 실패");
                    continue;
                }

                HexViewDataType dataType = new HexViewDataType();
                dataType.Alias = attrName.Value;
                dataType.Parent = parentDataType;

                if (parentDataType == null)
                {
                    // 최상위 노드이므로 _udds 에 추가

                    dataType.Name = dataType.Alias;
                    dataType.Alias = null;

                    _typeDefinitions.Add(dataType);
                }
                else
                {
                    if (parentDataType.Children == null)
                    {
                        parentDataType.Children = new List<HexViewDataType>();
                    }
                    parentDataType.Children.Add(dataType);
                }

                constructDataTypeArray(nodeType, dataType);

                constructDataTypeInternal(ref dataType, nodeType);
            }
        }

        private void constructDocumentTree(XmlNode xmlNode, ref TreeNode currTreeNode)
        {
            if (xmlNode != null)
            {
                for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
                {
                    XmlNode childNode = xmlNode.ChildNodes[i];

                    if (childNode.Attributes.Count == 0)
                    {
                        MessageBox.Show("속성이 없는 노드입니다. xml 파일을 확인해 주세요", "Document 파싱 실패");
                        continue;
                    }

                    TreeNode newChildNode = currTreeNode.appendChild(new NodeData());
                    for (int j = 0; j < childNode.Attributes.Count; ++j)
                    {
                        XmlNode currAttr = childNode.Attributes[j];
                        if (currAttr.Name == "Name")
                        {
                            newChildNode.Data.Name = currAttr.Value;
                        }
                        else if (currAttr.Name == "Type")
                        {
                            BuiltInDataType builtInType = HexViewDataType.stringToBuiltInDataType(currAttr.Value);
                            if (builtInType == BuiltInDataType.Wrapper)
                            {
                                HexViewDataType found = findDataType(currAttr.Value);
                                newChildNode.Data.Type = found;
                            }
                            else
                            {
                                newChildNode.Data.Type = new HexViewDataType(builtInType);
                            }
                        }
                        else if (currAttr.Name == "Content")
                        {
                            newChildNode.Data.Tip = currAttr.Value;
                        }
                    }
                    
                    constructDocumentTree(childNode, ref newChildNode);
                }
            }
        }
        
        private void construct(XmlNode xmlNode)
        {
            if (xmlNode.NodeType == XmlNodeType.Comment)
            {
                construct(xmlNode.NextSibling);
            }

            if (xmlNode.ChildNodes.Count > 0)
            {
                for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
                {
                    XmlNode currChildNode = xmlNode.ChildNodes[i];
                    if (currChildNode.Name == "TypeDefinition")
                    {
                        _typeDefinitions = new List<HexViewDataType>();
                        constructDataTypeArray(currChildNode, null);
                    }
                    else if (currChildNode.Name == "Document")
                    {
                        for (int j = 0; j < currChildNode.Attributes.Count; ++j)
                        {
                            XmlNode currAttr = currChildNode.Attributes[j];
                            if (currAttr.Name == "DefaultCodePage")
                            {
                                _defaultCodePage = int.Parse(currAttr.Value);
                            }
                        }
                        constructDocumentTree(currChildNode, ref _doc.getRootNode());
                    }
                }
            }
        }

        public bool OpenXml(string fileName)
        {
            bool existsFile = File.Exists(fileName);
            if (existsFile == false)
            {
                return false;
            }

            Stream fileStream = new FileStream(fileName, FileMode.Open);
            if (fileStream == null)
            {
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileStream);
            
            _doc = new Tree();
            _doc.makeRootNode(new NodeData("Document"));

            construct(doc.ChildNodes[0]);

            return true;
        }
    }
}
