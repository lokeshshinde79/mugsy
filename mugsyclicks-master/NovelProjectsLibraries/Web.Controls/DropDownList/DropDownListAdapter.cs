using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace NovelProjects.Web
{
	public class DropDownListAdapter : System.Web.UI.WebControls.Adapters.WebControlAdapter
	{
		private const string OptionGroupAttribute = "OptionGroup";
		private const string TagOptionGroup = "optgroup";
		private const string AttributeLabel = "label";

		#region Render Contents
		protected override void RenderContents(HtmlTextWriter writer)
		{
			DropDownList list = this.Control as DropDownList;
			string currentOptionGroup;
			List<string> renderedOptionGroups = new List<string>();

			foreach (ListItem item in list.Items)
			{
				Page.ClientScript.RegisterForEventValidation(list.UniqueID, item.Value);
				if (item.Attributes[OptionGroupAttribute] == null)
				{
					RenderListItem(item, writer);
				}
				else
				{
					currentOptionGroup = item.Attributes[OptionGroupAttribute];
					if (renderedOptionGroups.Contains(currentOptionGroup))
					{
						RenderListItem(item, writer);
					}
					else
					{
						if (renderedOptionGroups.Count > 0)
						{
							RenderOptionGroupEndTag(writer);
						}

						RenderOptionGroupBeginTag(currentOptionGroup, writer);
						renderedOptionGroups.Add(currentOptionGroup);
						RenderListItem(item, writer);
					}
				}
			}

			if (renderedOptionGroups.Count > 0)
			{
				RenderOptionGroupEndTag(writer);
			}
		}
		#endregion

		#region Render Option Group
		private void RenderOptionGroupBeginTag(string name, HtmlTextWriter writer)
		{
			writer.AddAttribute(AttributeLabel, name);
			writer.RenderBeginTag(TagOptionGroup);
		}

		private void RenderOptionGroupEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}
		#endregion

		#region Render List Item
		private void RenderListItem(ListItem item, HtmlTextWriter writer)
		{
			foreach (string key in item.Attributes.Keys)
			{
				if (key != OptionGroupAttribute)
					writer.AddAttribute(key, item.Attributes[key]);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Value, true);
			if (item.Selected)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Option);
			writer.WriteEncodedText(item.Text);
			writer.RenderEndTag();
		}
		#endregion

		#region Save ViewState
		protected override object SaveAdapterViewState()
		{
			DropDownList list = Control as DropDownList;
			object[] viewState = new object[list.Items.Count + 2];
			int i = 0;
			foreach (ListItem item in list.Items)
			{
				viewState[i++] = item.Attributes[OptionGroupAttribute];
			}
			viewState[i++] = base.SaveAdapterViewState();
			viewState[i] = Hash(list.Items);
			return viewState;
		}
		#endregion

		#region Load ViewState
		object[] viewStates;

		protected override void LoadAdapterViewState(object state)
		{
			viewStates = (object[])state;
			base.LoadAdapterViewState(viewStates[viewStates.Length - 1]);
		}

		protected override void OnPreRender(System.EventArgs e)
		{
			if (viewStates != null && viewStates.Length > 1)
			{
				DropDownList list = Control as DropDownList;
				if (Page.EnableEventValidation)
				{
					if (Hash(list.Items) != (int)viewStates[viewStates.Length - 1])
					{
						throw new ViewStateException();
					}
				}
				int max = viewStates.Length - 1;
				if (list.Items.Count < max)
				{
					max = list.Items.Count;
				}
				for (int i = 0; i < max; i++)
				{
					list.Items[i].Attributes[OptionGroupAttribute] = (string)viewStates[i];

				}
			}
			base.OnPreRender(e);
		}
		#endregion

		private static int Hash(ListItemCollection listItems)
		{
			int hash = 0;
			foreach (ListItem listItem in listItems)
				hash += listItem.GetHashCode();

			return hash;
		}
	}
}