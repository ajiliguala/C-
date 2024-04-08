using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200007A RID: 122
	public class OutInStockManager : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000590 RID: 1424 RVA: 0x000443B4 File Offset: 0x000425B4
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.View.GetControl<TabControl>("FBillList").SetFireSelChanged(true);
			this.View.GetControl("FTreeView").SetCustomPropertyValue("enableDD", true);
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x000443EC File Offset: 0x000425EC
		public override List<TreeNode> GetTreeViewData(TreeNodeArgs treeNodeArgs)
		{
			if (string.IsNullOrEmpty(treeNodeArgs.NodeId) || treeNodeArgs.NodeId == "0")
			{
				this._treeGroups = this.GetTreeGroup();
				this._currentNode = this._treeGroups.FirstOrDefault<TreeNode>();
				return this._treeGroups;
			}
			return null;
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0004443D File Offset: 0x0004263D
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			if (!this._treeNodes.ContainsKey(e.NodeId))
			{
				return;
			}
			this._currentNode = this._treeNodes[e.NodeId];
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0004446A File Offset: 0x0004266A
		public override void TreeNodeDoubleClick(TreeNodeArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FTREEVIEW"))
			{
				if (string.IsNullOrWhiteSpace(e.NodeId))
				{
					return;
				}
				this.ShowBillListPage(e.NodeId.Trim());
			}
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x000444A4 File Offset: 0x000426A4
		public override void TreeDragDrop(TreeDragDropEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.CurParentId) && !this._treeNodes.Keys.Contains(e.CurParentId))
			{
				return;
			}
			if (!this.CheckPermission("STK_OutInStock", "5add535f1bb250"))
			{
				this.View.ShowMessage(ResManager.LoadKDString("对不起，你没有添加单据的权限！", "004023000038313", 5, new object[0]), 0);
				return;
			}
			string curParentId = e.CurParentId;
			string ids = e.Ids;
			string text = string.IsNullOrWhiteSpace(this._treeNodes[curParentId].parentid) ? curParentId : this._treeNodes[curParentId].parentid;
			TreeNode treeNode = this._treeNodes[ids];
			if (!string.IsNullOrWhiteSpace(treeNode.parentid) && !text.Equals(treeNode.parentid))
			{
				this.RemoveNode(treeNode);
				treeNode.parentid = text;
				if (this.UpdateTreeData(treeNode, "ADDNODE"))
				{
					this.AddNewNodeToTree(treeNode);
				}
			}
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x000445B0 File Offset: 0x000427B0
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			if (this._formBillList.ContainsValue(e.TabIndex) && (!this._formBillList.ContainsKey(this._currentNode.id) || this._formBillList[this._currentNode.id] != e.TabIndex))
			{
				this._tabShowForm = this._formBillList.First((KeyValuePair<string, int> p) => p.Value == e.TabIndex).Key;
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0004465C File Offset: 0x0004285C
		public override void ToolBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBADDNODE"))
				{
					if (!(a == "TBDELNODE"))
					{
						if (!(a == "TBADDGROUP"))
						{
							if (!(a == "TBEDITGROUP"))
							{
								return;
							}
							string operateName = ResManager.LoadKDString("编辑分组", "004023030009244", 5, new object[0]);
							string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
							if (!string.IsNullOrWhiteSpace(onlyViewMsg))
							{
								e.Cancel = true;
								this.View.ShowErrMessage(onlyViewMsg, "", 0);
								return;
							}
							this.isAdd = false;
							if (this._currentNode == null || this._currentNode.xtype.Equals("leaf"))
							{
								this.View.ShowMessage(ResManager.LoadKDString("请先选一个类别！", "004023030002233", 5, new object[0]), 0);
								e.Cancel = true;
								return;
							}
							if (this._sysNode.Contains(this._currentNode.id))
							{
								this.View.ShowMessage(ResManager.LoadKDString("系统设置，不允许编辑！", "004023030002236", 5, new object[0]), 0);
								e.Cancel = true;
								return;
							}
							if (!this.CheckPermission("STK_OutInStock", "5add533e1bb24d"))
							{
								this.View.ShowMessage(ResManager.LoadKDString("对不起，你没有编辑分组的权限！", "004023000038315", 5, new object[0]), 0);
								e.Cancel = true;
								return;
							}
							DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
							dynamicFormShowParameter.FormId = "STK_OUTINBILLGROUP";
							dynamicFormShowParameter.ParentPageId = this.View.PageId;
							dynamicFormShowParameter.CustomParams.Add("_FormID", this._currentNode.id);
							dynamicFormShowParameter.CustomParams.Add("_Name", this._currentNode.text);
							this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AddGoupNode));
						}
						else
						{
							string operateName = ResManager.LoadKDString("新增分组", "004023030009243", 5, new object[0]);
							string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
							if (!string.IsNullOrWhiteSpace(onlyViewMsg))
							{
								e.Cancel = true;
								this.View.ShowErrMessage(onlyViewMsg, "", 0);
								return;
							}
							if (!this.CheckPermission("STK_OutInStock", "5add53061bb24a"))
							{
								this.View.ShowMessage(ResManager.LoadKDString("对不起，你没有新增分组的权限！", "004023000038282", 5, new object[0]), 0);
								e.Cancel = true;
								return;
							}
							this.isAdd = true;
							DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
							dynamicFormShowParameter.FormId = "STK_OUTINBILLGROUP";
							dynamicFormShowParameter.ParentPageId = this.View.PageId;
							this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AddGoupNode));
							return;
						}
					}
					else
					{
						string operateName = ResManager.LoadKDString("删除单据", "004023030009242", 5, new object[0]);
						string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
						if (!string.IsNullOrWhiteSpace(onlyViewMsg))
						{
							e.Cancel = true;
							this.View.ShowErrMessage(onlyViewMsg, "", 0);
							return;
						}
						if (this._currentNode == null || this._currentNode.id == null)
						{
							this.View.ShowMessage(ResManager.LoadKDString("请先选一个节点！", "004023030002224", 5, new object[0]), 0);
							e.Cancel = true;
							return;
						}
						if (this._sysNode.Contains(this._currentNode.id))
						{
							this.View.ShowMessage(ResManager.LoadKDString("系统设置，不允许删除！", "004023030002227", 5, new object[0]), 0);
							e.Cancel = true;
							return;
						}
						if (!this.CheckPermission("STK_OutInStock", "24f64c0dbfa945f78a6be123197a63f5"))
						{
							this.View.ShowMessage(ResManager.LoadKDString("对不起，你没有删除权限！", "004023000038314", 5, new object[0]), 0);
							e.Cancel = true;
							return;
						}
						this.View.ShowMessage(string.Format(ResManager.LoadKDString("是否确认删除{0}？", "004023030002230", 5, new object[0]), this._currentNode.text), 4, delegate(MessageBoxResult result)
						{
							if (result == 6)
							{
								this.RemoveNode(this._currentNode);
							}
						}, "", 0);
						return;
					}
				}
				else
				{
					string operateName = ResManager.LoadKDString("添加单据", "004023030009241", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						e.Cancel = true;
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					if (this._currentNode == null || this._currentNode.id == null)
					{
						this.View.ShowMessage(ResManager.LoadKDString("请先选一个节点！", "004023030002224", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					if (!this.CheckPermission("STK_OutInStock", "5add535f1bb250"))
					{
						this.View.ShowMessage(ResManager.LoadKDString("对不起，你没有添加单据的权限！", "004023000038313", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					ListShowParameter listShowParameter = new ListShowParameter();
					listShowParameter.MultiSelect = true;
					listShowParameter.ParentPageId = this.View.PageId;
					listShowParameter.FormId = "STK_StockBillList";
					listShowParameter.Width = 750;
					this.View.ShowForm(listShowParameter, new Action<FormResult>(this.ReturnData));
					return;
				}
			}
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00044B88 File Offset: 0x00042D88
		private List<TreeNode> GetTreeGroup()
		{
			DynamicObjectCollection stockBillsInManage = StockServiceHelper.GetStockBillsInManage(base.Context);
			foreach (DynamicObject dynamicObject in stockBillsInManage)
			{
				if (!this._treeNodes.ContainsKey(dynamicObject["FFORMID"].ToString().Trim()))
				{
					this._treeNodes.Add(dynamicObject["FFORMID"].ToString().Trim(), new TreeNode
					{
						id = dynamicObject["FFORMID"].ToString().Trim(),
						parentid = dynamicObject["FPARENTID"].ToString().Trim(),
						xtype = (string.IsNullOrWhiteSpace(dynamicObject["FPARENTID"].ToString().Trim()) ? "system" : "leaf"),
						text = dynamicObject["FNAME"].ToString().Trim(),
						children = this.GetSubNode(stockBillsInManage, dynamicObject["FFORMID"].ToString().Trim())
					});
				}
				if (Convert.ToInt32(dynamicObject["FISSYSTEM"].ToString()) == 1 && !this._sysNode.Contains(dynamicObject["FFORMID"].ToString().Trim()))
				{
					this._sysNode.Add(dynamicObject["FFORMID"].ToString().Trim());
				}
				if (Convert.ToInt32(dynamicObject["FISSYSTEM"].ToString()) != 1 && !this._formPermission.ContainsKey(dynamicObject["FFORMID"].ToString().Trim()) && dynamicObject["FPERMISSIONITEMID"] != null && !string.IsNullOrWhiteSpace(dynamicObject["FPERMISSIONITEMID"].ToString().Trim()))
				{
					this._formPermission.Add(dynamicObject["FFORMID"].ToString().Trim(), dynamicObject["FPERMISSIONITEMID"].ToString().Trim());
				}
			}
			List<TreeNode> list = new List<TreeNode>();
			return (from p in this._treeNodes
			where p.Value.xtype.Equals("system")
			select p.Value).ToList<TreeNode>();
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x00044EEC File Offset: 0x000430EC
		private List<TreeNode> GetSubNode(DynamicObjectCollection nodeList, string parentID)
		{
			return (from p in nodeList
			where p["FPARENTID"].ToString().Equals(parentID) && this.CheckPermission(p["FFORMID"].ToString(), p["FPERMISSIONITEMID"].ToString())
			select new TreeNode
			{
				id = p["FFORMID"].ToString(),
				parentid = p["FPARENTID"].ToString(),
				xtype = "leaf",
				text = p["FNAME"].ToString()
			}).ToList<TreeNode>();
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x00044F48 File Offset: 0x00043148
		private void ShowBillListPage(string nodeID)
		{
			if (!this._treeNodes.ContainsKey(nodeID))
			{
				return;
			}
			this._currentNode = this._treeNodes[nodeID];
			if (this._currentNode.xtype.Equals("leaf"))
			{
				this._tabShowForm = nodeID;
				if (this._formBillList.ContainsKey(nodeID))
				{
					this.View.GetControl<TabControl>("FBillList").SelectedIndex = this._formBillList[nodeID];
					return;
				}
				ListShowParameter listShowParameter = new ListShowParameter();
				if (nodeID.ToUpperInvariant().Equals("PUR_RECEIVEBILL"))
				{
					listShowParameter.ListFilterParameter.Filter = string.Format("t0.FBUSINESSTYPE = N'{0}' OR t0.FBUSINESSTYPE = N'{1}'", "CG", "WW");
				}
				listShowParameter.FormId = nodeID;
				listShowParameter.ParentPageId = this.View.PageId;
				listShowParameter.PageId = nodeID + SequentialGuid.NewGuid().ToString();
				listShowParameter.MultiSelect = true;
				listShowParameter.IsShowApproved = false;
				listShowParameter.OpenStyle.CacheId = listShowParameter.PageId;
				listShowParameter.OpenStyle.TagetKey = "FBillList";
				listShowParameter.OpenStyle.ShowType = 1;
				listShowParameter.PermissionItemId = (this._formPermission.ContainsKey(nodeID) ? this._formPermission[nodeID] : "6e44119a58cb4a8e86f6c385e14a17ad");
				listShowParameter.CustomParams.Add("EnableCustomDefaultScheme", "true");
				this.View.ShowForm(listShowParameter, new Action<FormResult>(this.CloseFun));
				this._formBillList.Add(nodeID, this._formBillList.Count);
			}
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x000450F0 File Offset: 0x000432F0
		private void CloseFun(FormResult data)
		{
			if (this._formBillList.ContainsKey(this._tabShowForm))
			{
				this._formBillList.Remove(this._tabShowForm);
				string[] array = (from p in this._formBillList
				orderby p.Value
				select p.Key).ToArray<string>();
				for (int i = 0; i < array.Length; i++)
				{
					this._formBillList[array[i]] = i;
					if (i == array.Length - 1)
					{
						this.ShowBillListPage(array[i]);
					}
				}
			}
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x000451A4 File Offset: 0x000433A4
		private bool CheckPermission(string formkey, string perItemId = "6e44119a58cb4a8e86f6c385e14a17ad")
		{
			if (string.IsNullOrEmpty(formkey))
			{
				return false;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = formkey
			}, perItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x000451E4 File Offset: 0x000433E4
		private void ReturnData(FormResult result)
		{
			if (result.ReturnData != null)
			{
				ListSelectedRowCollection listSelectedRowCollection = (ListSelectedRowCollection)result.ReturnData;
				OperateResultCollection operateResultCollection = new OperateResultCollection();
				foreach (ListSelectedRow listSelectedRow in listSelectedRowCollection)
				{
					if (!this.CheckPermission(listSelectedRow.PrimaryKeyValue, "6e44119a58cb4a8e86f6c385e14a17ad"))
					{
						operateResultCollection.Add(new OperateResult
						{
							Name = ResManager.LoadKDString("无单据权限", "004023030002239", 5, new object[0]),
							Message = string.Format(ResManager.LoadKDString("无单据[{0}]权限", "004023030002242", 5, new object[0]), listSelectedRow.Name),
							SuccessStatus = false
						});
					}
					else if (this._treeNodes.ContainsKey(listSelectedRow.PrimaryKeyValue))
					{
						string text = this._treeNodes[this._treeNodes[listSelectedRow.PrimaryKeyValue].parentid].text;
						operateResultCollection.Add(new OperateResult
						{
							Name = ResManager.LoadKDString("已经存在", "004023030002245", 5, new object[0]),
							Message = string.Format(ResManager.LoadKDString("根节点{0}下已存在{1}", "004023030002248", 5, new object[0]), text, listSelectedRow.Name),
							SuccessStatus = false
						});
					}
					else
					{
						TreeNode treeNode = new TreeNode
						{
							id = listSelectedRow.PrimaryKeyValue,
							text = listSelectedRow.Name,
							xtype = "leaf",
							parentid = (this._currentNode.xtype.Equals("leaf") ? this._currentNode.parentid : this._currentNode.id)
						};
						if (this.UpdateTreeData(treeNode, "ADDNODE"))
						{
							this.AddNewNodeToTree(treeNode);
						}
					}
				}
				if (operateResultCollection.Count > 0)
				{
					this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
				}
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x00045400 File Offset: 0x00043600
		private void RemoveNode(TreeNode currentNode)
		{
			if (currentNode.xtype.Equals("leaf"))
			{
				if (this.UpdateTreeData(currentNode, "DELNODE"))
				{
					this.RemoveNodeFromTree(currentNode);
				}
			}
			else
			{
				List<TreeNode> list = new List<TreeNode>(currentNode.children);
				foreach (TreeNode currentNode2 in list)
				{
					this.RemoveNode(currentNode2);
				}
				if (this.UpdateTreeData(currentNode, "DELNODE"))
				{
					this.RemoveNodeFromTree(currentNode);
				}
			}
			this._currentNode = this._treeGroups.FirstOrDefault<TreeNode>();
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x000454AC File Offset: 0x000436AC
		private bool UpdateTreeData(TreeNode node, string UpdateType)
		{
			int num = StockServiceHelper.UpdateStockBillInManage(base.Context, node, UpdateType, null, null);
			if (num == 101)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("系统设置，不允许删除!", "004023030002227", 5, new object[0]), "", 0);
				return false;
			}
			return true;
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x000454F8 File Offset: 0x000436F8
		private void AddGoupNode(FormResult result)
		{
			if (result.ReturnData != null)
			{
				TreeNode treeNode = result.ReturnData as TreeNode;
				if (this.isAdd)
				{
					this.AddNewNodeToTree(treeNode);
					return;
				}
				foreach (TreeNode treeNode2 in this._currentNode.children)
				{
					treeNode2.parentid = treeNode.id;
					treeNode.children.Add(treeNode2);
				}
				this.RemoveNodeFromTree(this._currentNode);
				this.AddNewNodeToTree(treeNode);
				this._currentNode = treeNode;
			}
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x000455A4 File Offset: 0x000437A4
		private void RemoveNodeFromTree(TreeNode node)
		{
			if (node.xtype.Equals("system"))
			{
				this._treeGroups.Remove(node);
			}
			else
			{
				this._treeNodes[node.parentid].children.Remove(node);
			}
			this._treeNodes.Remove(node.id);
			this.View.GetControl<TreeView>("FTreeView").RemoveNode(node.id);
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0004561C File Offset: 0x0004381C
		private void AddNewNodeToTree(TreeNode newNode)
		{
			this._treeNodes.Add(newNode.id, newNode);
			this.View.GetControl<TreeView>("FTreeView").AddNode(newNode);
			if (newNode.xtype.Equals("system"))
			{
				this._treeGroups.Add(newNode);
				return;
			}
			this._treeNodes[newNode.parentid].children.Add(newNode);
		}

		// Token: 0x04000220 RID: 544
		private Dictionary<string, TreeNode> _treeNodes = new Dictionary<string, TreeNode>();

		// Token: 0x04000221 RID: 545
		private List<TreeNode> _treeGroups = new List<TreeNode>();

		// Token: 0x04000222 RID: 546
		private TreeNode _currentNode = new TreeNode();

		// Token: 0x04000223 RID: 547
		private string _tabShowForm = string.Empty;

		// Token: 0x04000224 RID: 548
		private Dictionary<string, int> _formBillList = new Dictionary<string, int>();

		// Token: 0x04000225 RID: 549
		private List<string> _sysNode = new List<string>();

		// Token: 0x04000226 RID: 550
		private bool isAdd;

		// Token: 0x04000227 RID: 551
		private Dictionary<string, string> _formPermission = new Dictionary<string, string>();
	}
}
