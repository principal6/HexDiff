using System.Windows.Forms;
using TreeStructure;
using HexViewProject;
using System.Collections.Generic;
using System;

namespace HexDiff
{
    using HexViewTreeNode = Tree<NodeData>.Node;

    public partial class HexDiffTreeView : UserControl
    {
        private enum Column
        {
            Field,
            Content,
            COUNT
        }

        private enum Field
        {
            Name,
            Type,
            Size,
            Value,
            Tip,
            COUNT
        }

        HexView _hexView;
        HexViewTreeNode _selectedNode;

        public HexDiffTreeView()
        {
            InitializeComponent();

            listView.Columns.Clear();
            listView.Columns.Add("Field");
            listView.Columns.Add("Content");
            listView.Columns[0].Width = 80;
            listView.Columns[1].Width = listView.Width - listView.Columns[0].Width - 110;
            
            listView.Items.Clear();
            for (int i = 0; i < (int)Field.COUNT; ++i)
            {
                ListViewItem newItem = new ListViewItem();
                newItem.Text = ((Field)i).ToString();
                newItem.SubItems.Add("test");
                listView.Items.Add(newItem);
            }
        }

        private void constructTreeViewDataType(TreeNode currTreeViewNode, HexViewDataType currDataType)
        {
            if (currDataType == null)
            {
                return;
            }

            currTreeViewNode = currTreeViewNode.Nodes.Add(currDataType.Alias);

            if (currDataType.Children != null)
            {
                foreach (HexViewDataType childDataType in currDataType.Children)
                {
                    constructTreeViewDataType(currTreeViewNode, childDataType);
                }
            }
        }

        private void constructTreeView(TreeNode currTreeViewNode, HexViewTreeNode currHvNode)
        {
            if (currHvNode == null)
            {
                return;
            }

            if (currHvNode.Children != null)
            {
                foreach (HexViewTreeNode childHvNode in currHvNode.Children)
                {
                    TreeNode childNode = currTreeViewNode.Nodes.Add(childHvNode.Data.Name);
                    constructTreeView(childNode, childHvNode);
                }
                return;
            }
            
            if (currHvNode.Data.Type != null)
            {
                if (currHvNode.Data.Type.isBuiltInDataType() == false)
                {
                    if (currHvNode.Data.Type.Children != null)
                    {
                        foreach (HexViewDataType childDataType in currHvNode.Data.Type.Children)
                        {
                            constructTreeViewDataType(currTreeViewNode, childDataType);
                        }
                    }
                    
                }
            }
        }

        public bool loadHexViewFromFile(string fileName)
        {
            _hexView = new HexView();
            if (_hexView.OpenXml(fileName) == true)
            {
                treeView.Nodes.Clear();

                HexViewTreeNode hvRootNode = _hexView._doc.getRootNode();

                TreeNode treeViewRootNode = new TreeNode(hvRootNode.Data.Name);
                for (int i = 0; i < hvRootNode.Children.Count; ++i)
                {
                    HexViewTreeNode childHvNode = hvRootNode.Children[i];
                    TreeNode childNode = treeViewRootNode.Nodes.Add(childHvNode.Data.Name);
                    constructTreeView(childNode, childHvNode);
                }

                treeView.Nodes.Add(treeViewRootNode);

                return true;
            }
            return false;
        }

        private void updateListView()
        {
            if (_selectedNode != null)
            {
                listView.Items[(int)Field.Name].SubItems[1].Text = _selectedNode.Data.Name;
                listView.Items[(int)Field.Tip].SubItems[1].Text = _selectedNode.Data.Tip;

                if (_selectedNode.Data.Type != null)
                {
                    listView.Items[(int)Field.Type].SubItems[1].Text = _selectedNode.Data.Type.Name;
                    listView.Items[(int)Field.Value].SubItems[1].Text = _selectedNode.Data.Type.Format;
                    listView.Items[(int)Field.Size].SubItems[1].Text = _selectedNode.Data.Type.ByteCount.ToString();
                }
                else
                {
                    listView.Items[(int)Field.Type].SubItems[1].Text = null;
                    listView.Items[(int)Field.Value].SubItems[1].Text = null;
                    listView.Items[(int)Field.Size].SubItems[1].Text = null;
                }
            }
        }

        private void updateSelectedNode(TreeNode currTreeViewNode)
        {
            if (currTreeViewNode == null)
            {
                return;
            }

            updateSelectedNode(currTreeViewNode.Parent);

            if (currTreeViewNode.Parent == null) // 루트 노드일 때
            {
                _selectedNode = _hexView._doc.getRootNode();
            }
            else
            {
                if (_selectedNode.Children != null)
                {
                    _selectedNode = _selectedNode.Children[currTreeViewNode.Index];
                }
                else
                {
                    HexViewTreeNode newSelectedNode = new HexViewTreeNode();
                    newSelectedNode = new HexViewTreeNode();
                    newSelectedNode.Data = new NodeData();
                    newSelectedNode.Data.Type = _selectedNode.Data.Type.Children[currTreeViewNode.Index];
                    newSelectedNode.Data.Name = _selectedNode.Data.Name + "->" + newSelectedNode.Data.Type.Alias;
                    _selectedNode = newSelectedNode;
                }
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            updateSelectedNode(treeView.SelectedNode);

            updateListView();
        }
    }
}
