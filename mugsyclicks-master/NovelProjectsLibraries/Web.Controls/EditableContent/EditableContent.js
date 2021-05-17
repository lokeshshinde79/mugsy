function EnableApprove()
{
	$('.ApproveEditableContent').removeAttr('disabled');
}
function FCKeditor_OnComplete(editor) {
	editor.Events.AttachEvent( 'OnSelectionChange', EnableApprove );
}

$(function() {

	BindFunctions();
	
});

function BindFunctions() {
	var ContentID, EditorID;
	
	$('div.EditableTooltip:not(.tooltip)').tooltip({
		track:true, delay: 0, showURL: false, showBody: ' - '
	});
	
	$('div.EditableTooltip').unbind('dblclick').dblclick(function() {
		var $this = $(this);
		$('.DisableSaveEditableContent').hide();
		$('.SaveEditableContent').show();
		if ($('#Popup_' + $this.attr('id')).length>1)
			$('#Popup_' + $this.attr('id') + ':first').remove();
		
		$this.find('div.EditableContent:first').find('.npTabs').tabs();

		$('#Popup_' + $this.attr('id')).npModal({ width: '780px', height: '545px', sender:'#SiteManagerPopup', toggleHide:true,overlayClass:'.EditContentOverlay' });
		ContentID = $this.find('span.ContentID:first').text();
		
		EditorID = $('span.Editor_' + ContentID).text();
	});
	
//	$('.EditableTooltip a:not(.EditableContent a)').click(function(e){
//		if (!e.shiftKey)
//			return false;
//	});

	$('div.EditableTooltip').unbind('hover').hover(function() {
		$(this).removeClass('EditBoxOff').addClass('EditBoxOn');
	},
	function() {
		$(this).removeClass('EditBoxOn').addClass('EditBoxOff');
	});

	$('input.SaveEditableContent').unbind('click').click(function() {
		var $this = $(this);
		var versionid = $this.closest('.npTabs').find('.VersionId').html();
		$this.hide();
		$('.DisableSaveEditableContent').show();
		
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/SaveApprove',
			data: "{ ID:'" + versionid + "', ContentID:'" + ContentID + "', Content:'" + FormatContent() + "'," + 
				" Approve:'" + $this.nextAll('.ApproveEditableContent:first').find('input').is(':checked') + "'," +
				" IsNew:'" + $this.nextAll('.SaveNewContent:first').find('input').is(':checked') + "', PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				if (object!='Success')
					alert(object);
				
				location.href = location.href;
			},
			error: function(xhr, msg) {
				alert('Error saving content');
			}
		});
		return false;
	});

	$('a.btnCreateNewVersion').die('click').live('click', function() {
		var $this = $(this);
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/CreateNew',
			data: "{ ContentID:'" + ContentID + "', PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				var str =(object);
				var guid = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/
				var bresults

				if (guid.test(str))
				{
					// Set the version id of the edit
					$this.closest('.npTabs').find('.VersionId').html(str);
					$this.closest('.npTabs').find('.modifyapprove').hide();
					
					var oEditor = FCKeditorAPI.GetInstance($this.closest('.HistoryPages').attr('editorid'));
					oEditor.SetHTML("");
					$this.closest('.npTabs').tabs('select',0);
					
					var d = new Date();
					$this.replaceWith(d.getMonth() + "/" + d.getDay() + "/" + d.getFullYear() + " " + d.toLocaleTimeString());
				}
				else
				{
					alert(object);
				}
			},
			error: function(xhr, msg) {
				alert('Error creating new version');
			}
		});
		return false;
	});

	$('input.CancelEditableContent').unbind('click').click(function() {
		if ($('#SiteManagerPopup').size()==1)
			$(this).closest('div.EditableContent').npModalDestroy({ destroyOverlay:false });
		else
			location.href = location.href;
		return false;
	});

	$('input.btnApproveContent').unbind('click').click(function() {
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/Approve',
			data: "{ ID:'" + $(this).attr('id') + "', NewSave:'false', Content:'', ContentID:'" + ContentID + "'," +
				" PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				if (object!='Success')
					alert(object);
					
				location.href = location.href;
			},
			error: function(xhr, msg) {
				alert('Error approving content');
			}
		});
	});

	$('input.btnPublishContent').unbind('click').click(function() {
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/Publish',
			data: "{ ID:'" + $(this).attr('id') + "', PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				if (object!='Success')
					alert(object);
					
				location.href = location.href;
			},
			error: function(xhr, msg) {
				alert('Error publishing content');
			}
		});
	});

	$('input.EditableName').unbind('blur').blur(function() {
		var $this = $(this);
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/SaveName',
			data: "{ ID:'" + $this.attr('id') + "', Name:'" + $this.val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				if (object!='Success')
					alert(object);
			},
			error: function(xhr, msg) {
				alert('Error saving version name');
			}
		});
	});

	$('img.btnEditContent').unbind('click').click(function() {
		var $this = $(this);
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/LoadContent',
			data: "{ ID:'" + $this.attr('id') + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				// Set the version id of the edit
				$this.closest('.npTabs').find('.VersionId').html($this.attr('id'));
				$this.closest('.npTabs').find('.modifyapprove').hide();
				
				var oEditor = FCKeditorAPI.GetInstance($this.closest('.HistoryPages').attr('editorid'));
				oEditor.SetHTML(object);
				$this.closest('.npTabs').tabs('select',0);
			},
			error: function(xhr, msg) {
				alert('Error editing content');
			}
		});
	});
	
	$('img.btnCopyContent').unbind('click').click(function(){
		var $this = $(this);
		$.ajax({
			type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
			url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/CopyContent',
			data: "{ ID:'" + $this.attr('id') + "', ContentID:'" + ContentID + "', PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
			success: function(msg) {
				var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
				
				var str =(object);
				var guid = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/
				var bresults

				if (guid.test(str))
				{
					$.ajax({
						type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
						url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/LoadContent',
						data: "{ ID:'" + str + "' }",
						success: function(msg) {
							var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
							
							// Set the version id of the edit
							$this.closest('.npTabs').find('.VersionId').html(str);
							$this.closest('.npTabs').find('.modifyapprove').hide();
							
							var oEditor = FCKeditorAPI.GetInstance($this.closest('.HistoryPages').attr('editorid'));
							oEditor.SetHTML(object);
							$this.closest('.npTabs').tabs('select',0);
						},
						error: function(xhr, msg) {
							alert('Error loading new copy');
						}
					});
				}
				else
				{
					alert(object);
				}
			},
			error: function(xhr, msg) {
				alert('Error copying version');
			}
		});
		return false;
	});

	$('img.btnDeleteContent').unbind('click').click(function() {
		var $this = $(this);
		var answer = confirm("Are you sure you want to delete this revision? This action is irreversible.")
		if (answer){
			$.ajax({
				type: 'POST', dataType: 'json', contentType: 'application/json; charset=utf-8',
				url: 'NovelProjects.Web.EditableContent.EditableContentService.asmx/Delete',
				data: "{ ID:'" + $this.attr('id') + "', PageUrl:'" + $('#SiteManagerPopup .NodeUrl').val() + "' }",
				success: function(msg) {
					var object = (msg.d==undefined || msg.d==null) ? msg : msg.d;
					
					if (object!='Success')
					{
						alert(object);
					}
					else
					{
						var css = "";
						if ($this.closest('tr').hasClass('alt')) css = "alt";
						
						$this.closest('tr').replaceWith('<tr class="' + css + '"><td><a href="javascript:void(0);" class="btnCreateNewVersion">Create New</a></td><td/><td/><td/><td/><td/><td/><td/></tr>');
					}
				},
				error: function(xhr, msg) {
					alert('Error deleting content');
				}
			});
		}
	});
	
	function FormatContent()
	{
		return escape(FCKeditorAPI.GetInstance(EditorID).GetHTML()).replace(/\+/g,'%2B');
	};
}