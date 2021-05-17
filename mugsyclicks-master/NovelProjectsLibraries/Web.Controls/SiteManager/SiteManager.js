var NodeData;
var selectedNode;

function LoadNodeData(Url) {
  var timestamp = new Date().getTime();
  $.ajax({
    type: "POST", dataType: "json", contentType: "application/json; charset=utf-8",
    url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/LoadNode",
    data: "{ Url:'" + Url + "', TimeStamp:'" + timestamp + "' }",
    success: function(msg) {
      var object = (msg.d == undefined || msg.d == null) ? msg : msg.d;
      NodeData = object.parseJSON();

      $('.NodeUrl').val(NodeData.Url);
      $('.NodeTitle').val(NodeData.Title);
      $('.NodeDescription').val(NodeData.Description);
      $('.NodeApprovalEmail').val(NodeData.ApprovalEmail);
      $('.NodeSEOKeywords').val(NodeData.SEOKeywords);
      $('.NodeSEODescription').val(NodeData.SEODescription);
      $('.NodeNavItem input:checkbox').attr('checked', (NodeData.NavItem == "True") ? 'checked' : '');
      $('.NodeUseSSL input:checkbox').attr('checked', (NodeData.UseSSL == "True") ? 'checked' : '');
      $('.NodeHidden input:checkbox').attr('checked', (NodeData.Hidden == "True") ? 'checked' : '');
      $('.NodeAllowChildren input:checkbox').attr('checked', (NodeData.AllowChildren == "True") ? 'checked' : '');

      LoadRoles(NodeData.AccessRoles.split(','), '.NodeAccessRoles', 'Everyone');
      LoadRoles(NodeData.AllRoles.split(','), '.NodeAllRoles');
      LoadRoles(NodeData.EditRoles.split(','), '.NodeEditRoles', 'No one');
      LoadRoles(NodeData.AllRoles2.split(','), '.NodeAllRoles2');
      LoadRoles(NodeData.ApproveRoles.split(','), '.NodeApproveRoles', 'Use edit roles');
      LoadRoles(NodeData.AllRoles3.split(','), '.NodeAllRoles3');

      $('.BtnEditNode').show();
      $('.TemplateRow').hide();
      if (NodeData.IsAppFile == "False") $('#SiteManagerPopup .BtnDeleteNode').show();
      else $('#SiteManagerPopup .BtnDeleteNode').hide();
      if (NodeData.AllowChildren == "True") {
        $('#SiteManagerPopup .BtnAddFile').show();
        $('#SiteManagerPopup .BtnAddFolder').show();
        $('.EditRolesRow').hide();
        $('.ApproveRolesRow').hide();
        $('.FolderViewRow').hide();
        $('.PageTitleText').text('Nav Title: ');
        $('#SMProperties').css('height', '114px');
        $('#SMAccess').css('height', '142px');
      }
      else {
        $('#SiteManagerPopup .BtnAddFile').hide();
        $('#SiteManagerPopup .BtnAddFolder').hide();
        $('.EditRolesRow').show();
        $('.ApproveRolesRow').show();
        $('.FolderViewRow').show();
        $('.PageTitleText').text('Page Title: ');
        $('#SMProperties').css('height', '390px');
        $('#SMAccess').css('height', '418px');
      }
      $('.NodeUrl').attr('disabled', true);
      $('.UrlText').text('');
      $('.UrlName').text('Location');
      $('#SiteManagerPopup .npTitle>h1').text('Edit: ' + NodeData.Title);

      $('.SMTabs').tabs("select", 0);
      if (NodeData.EditRoles != "") {
        $('.SMEditContentTab').show();
        $('#SMIframe').html('');
        $.post(rootpath + Url, { action: "get" }, function(data) {
          $(data).find('.EditableTooltip').each(function(i) {
            $('<p class="npSpace">&nbsp;</p><span class="ContentHeading">Section: ' + $(this).attr('id') + '</span><p class="npSpace">&nbsp;</p>').appendTo('#SMIframe');
            $(this).appendTo('#SMIframe');
          });
          BindFunctions();
        });
      }
      else {
        $('.SMEditContentTab').hide();
      }
      $('#SiteManagerPopup').npModal({ width: '450px', height: (NodeData.AllowChildren == "True" ? '254px' : '530px'), overlayClass: '.EditContentOverlay' });
    },
    error: function(xhr, msg) {
      alert("fail: " + msg + '\n ' + xhr.responseText);
    }
  });
}

function LoadRoles(roles, id, initial) {
  $(id).empty();
  if (roles.toString().length == 0) {
    $(id).append(MakeOption(initial, '', true));
  }

  $.each(roles, function() {
    if (this != "") {
      $(id).append(MakeOption(this, this, false));
    }
  });
}

function MakeOption(text, value, disabled) {
  var thisOpt = document.createElement('option');
  thisOpt.value = value;
  thisOpt.disabled = disabled;
  if ($.browser.msie && disabled) $(thisOpt).css('color', '#CCC');
  thisOpt.appendChild(document.createTextNode(text));

  return thisOpt;
}

function ClearValues(allowChildren) {
  $('.NodeUrl').removeAttr('disabled').val("");
  $('.NodeTitle').val("");
  $('.NodeDescription').val("");
  $('.NodeApprovalEmail').val("");
  $('.NodeSEOKeywords').val("");
  $('.NodeSEODescription').val("");
  $('.NodeNavItem input:checkbox').removeAttr('checked');
  $('.NodeUseSSL input:checkbox').removeAttr('checked');
  $('.NodeHidden input:checkbox').removeAttr('checked');

  if (allowChildren)
    $('.NodeAllowChildren input:checkbox').attr('checked', 'checked');
  else
    $('.NodeAllowChildren input:checkbox').removeAttr('checked');
}

function AddFolder($this, location) {
  ClearValues(true);

  $('.SMTabs').tabs("select", 0);
  $('.SMEditContentTab').hide();
  $('.TemplateRow').hide();
  $('.EditRolesRow').hide();
  $('.ApproveRolesRow').hide();
  $('.FolderViewRow').hide();
  $('.PageTitleText').text('Nav Title: ');
  $('#SMProperties').css('height', '114px');
  $('#SMAccess').css('height', '142px');

  $('#SiteManagerPopup .BtnDeleteNode').hide();
  $('#SiteManagerPopup .BtnAddFile').hide();
  $('#SiteManagerPopup .BtnAddFolder').hide();

  if ($this.parent().hasClass('root')) {
    $('#SiteManagerPopup .npTitle>h1').text('Add Folder');
  } else if ($this.attr('url') != undefined) {
    $('.UrlText').text($this.attr('url'));
    $('#SiteManagerPopup .npTitle>h1').text('Add Folder: ' + $this.attr('url'));
  } else {
    $('.UrlText').text(location);
    $('#SiteManagerPopup .npTitle>h1').text('Add Folder: ' + location);
  }
  $('.UrlName').text('Folder Name');
  $('#SiteManagerPopup').npModal({ width: '450px', height: '254px', overlayClass: '.EditContentOverlay' });

  $.ajax({
    type: "POST", dataType: "json", contentType: "application/json; charset=utf-8",
    url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/AddNode",
    data: "{}",
    success: function(msg) {
      var object = msg.d;
      if (object == undefined || object == null) object = msg;
      object = object.parseJSON();

      LoadRoles(object.AccessRoles.split(','), '.NodeAccessRoles', 'Everyone');
      LoadRoles(object.AllRoles.split(','), '.NodeAllRoles');
    },
    error: function(xhr, msg) { }
  });
}

function AddFile($this, location) {
  $('.BtnEditContent').hide();
  $('.TemplateRow').show();
  $('.NodeTemplate').val('');
  $('.EditRolesRow').show();
  $('.ApproveRolesRow').show();
  $('.FolderViewRow').show();
  $('.PageTitleText').text('Page Title: ');
  $('#SMProperties').css('height', '390px');
  $('#SMAccess').css('height', '418px');

  ClearValues(false);

  $('.SMTabs').tabs("select", 0);
  $('.SMEditContentTab').hide();
  $('#SiteManagerPopup .BtnDeleteNode').hide();
  $('#SiteManagerPopup .BtnAddFile').hide();
  $('#SiteManagerPopup .BtnAddFolder').hide();

  //alert($this.parent().hasClass('root'));
  if ($this.parent().hasClass('root')) {
    $('#SiteManagerPopup .npTitle>h1').text('Add File');
    $('#SiteManagerPopup .BtnDeleteNode').hide();
    $('#SiteManagerPopup .BtnAddFile').hide();
    $('#SiteManagerPopup .BtnAddFolder').hide();
  } else if ($this.attr('url') != undefined) {
    $('.UrlText').text($this.attr('url'));
    $('#SiteManagerPopup .npTitle>h1').text('Add File: ' + $this.attr('url'));
  } else {
    $('.UrlText').text(location);
    $('#SiteManagerPopup .npTitle>h1').text('Add File: ' + location);
  }
  $('.UrlName').text('File Name');
  $('#SiteManagerPopup').npModal({ width: '450px', height: '530px', overlayClass: '.EditContentOverlay' });

  $.ajax({
    type: "POST", dataType: "json", contentType: "application/json; charset=utf-8",
    url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/AddNode",
    data: "{}",
    success: function(msg) {
      var object = msg.d;
      if (object == undefined || object == null) object = msg;
      object = object.parseJSON();

      LoadRoles(object.AccessRoles.split(','), '.NodeAccessRoles', 'Everyone');
      LoadRoles(object.AllRoles.split(','), '.NodeAllRoles');
      LoadRoles(object.EditRoles.split(','), '.NodeEditRoles', 'No one');
      LoadRoles(object.AllRoles2.split(','), '.NodeAllRoles2');
      LoadRoles(object.ApproveRoles.split(','), '.NodeApproveRoles', 'Use edit roles');
      LoadRoles(object.AllRoles3.split(','), '.NodeAllRoles3');
    },
    error: function(xhr, msg) { }
  });
}

function DeleteNode(NodeUrl) {
  var answer = confirm("Are you sure you want to delete this item? This action is irreversible.");
  if (answer) {
    $.ajax({
      type: "POST", dataType: "json",
      url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/DeleteNode",
      data: "{ Url:'" + NodeUrl + "' }",
      contentType: "application/json; charset=utf-8",
      success: function(msg) {
        location.href = location.href;
      },
      error: function(xhr, msg) {
        alert("fail: " + msg + '\n ' + xhr.responseText);
      }
    });
  }
}

$(function() {
  $('.SMtooltip').tooltip({ delay: 0, showURL: false, track: true, fade: 250, showBody: "|" });
  $('.SMTabs').tabs({
    select: function(event, ui) {
      var smp = $('#SiteManagerPopup');

      if (ui.index == 2) {
        $('.BtnHolder').hide();
        smp.css('left', (parseInt(smp.css('left'), 10) - 220) + 'px');
        smp.css('width', '840px');
      }
      else {
        $('.BtnHolder').show();
        if (parseInt(smp.css('width')) != 450) {
          smp.css('left', (parseInt(smp.css('left'), 10) + 220) + 'px');
          smp.css('width', '450px');
        }
      }
    }
  });

  // click doesn't work in IE with jquery 1.3.2 or 1.4.2
  //$('.treenode').click(function() {

  // this works in IE, Firefox, Chrome with jquery 1.4.2
  //$('.treenode').live("click", function() {

  $('.treenode').live("click", function() {
    selectedNode = $(this);
    $('.selected').removeClass('selected');
    selectedNode.addClass('selected');
    LoadNodeData($(this).attr('url'));
  });

  $('#treestuff').tree({
    rules: {
      draggable: 'all',
      drag_button: "left",
      dragrules: ["folder before folder", "folder after folder", "folder before file", "folder after file", "file after file", "file before file", "file after folder", "file before folder"]
    },
    ui: {
      context: [
				{
				  id: "addfile",
				  label: "Add File",
				  //icon: "../media/images/ok.png",
				  visible: function(NODE, TREE_OBJ) {
				    // return -1 for not visible
				    // return true for visible and clickable
				    // return false for disabled
				    if (NODE.attr("rel") == "folder") return true;
				    return -1;
				  },
				  action: function(NODE, TREE_OBJ) {
				    AddFile($(NODE).children('a:first'));
				  }
				},
				{
				  id: "addfolder",
				  label: "Add Folder",
				  //icon: "../media/images/ok.png",
				  visible: function(NODE, TREE_OBJ) {
				    // return -1 for not visible
				    // return true for visible and clickable
				    // return false for disabled
				    if (NODE.attr("rel") == "folder") return true;
				    return -1;
				  },
				  action: function(NODE, TREE_OBJ) {
				    AddFolder($(NODE).children('a:first'));
				  }
				},
				{
				  id: "delete",
				  label: "Delete",
				  //icon: "../media/images/ok.png",
				  visible: function(NODE, TREE_OBJ) {
				    // return -1 for not visible
				    // return true for visible and clickable
				    // return false for disabled
				    if (NODE.attr("deletable") == "True") return -1;
				    return true;
				  },
				  action: function(NODE, TREE_OBJ) {
				    DeleteNode($(NODE).children('a:first').attr('url'));
				  }
				}
			]
    },
    callback: {
      beforemove: function(node, ref_node, type, tree_obj) {
        var parenturl = $(node).parent().parent().children('a:first').attr('url');
        var parentrefurl = $(ref_node).parent().parent().children('a:first').attr('url');

        if (parenturl != parentrefurl)
          return false;
        else
          return true;
      },
      onmove: function(node, ref_node, type, tree_obj) {
        var url = $(node).find('a:first').attr('url');
        var refurl = $(ref_node).find('a:first').attr('url');

        $.ajax({
          type: "POST", dataType: "json",
          url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/MoveNode",
          data: "{ Url:'" + url + "'," + "RefUrl:'" + refurl + "'," + "Type:'" + type + "' }",
          contentType: "application/json; charset=utf-8",
          success: function(msg) {
          },
          error: function(xhr, msg) {
            location.href = location.href;
          }
        });
      }
    }
  });

  $('.BtnSaveNode').click(function() {
    if (Page_ClientValidate("SiteManager")) {
      $.ajax({
        type: "POST", dataType: "json", contentType: "application/json; charset=utf-8",
        url: "NovelProjects.Web.SiteManager.SiteManagerService.asmx/SaveNode",
        data: formJSONNode(),
        success: function(msg) {
          var object = (msg.d == undefined || msg.d == null) ? msg : msg.d;
          if (object == "Trouble saving file.") alert(object);
          else NodeData = object.parseJSON();
          if (!$('#SiteManagerPopup .NodeUrl').is(':disabled')) {
            location.href = location.href;
            return;
          }

          if (selectedNode != undefined)
            selectedNode.text(NodeData.Title);

          $('#SiteManagerPopup').npModalDestroy({ overlayClass: '.EditContentOverlay' });
        },
        error: function(xhr, msg) {
          var object = xhr.responseText;
          object = object.parseJSON();
          alert(object.Message);
        }
      });
    }
  });

  $('.BtnAddFolder').click(function() {
    AddFolder($(this), $('.NodeUrl').val());
  });

  $('.BtnAddFile').click(function() {
    AddFile($(this), $('.NodeUrl').val());
  });

  $('.BtnDeleteNode').click(function() {
    DeleteNode($('.NodeUrl').val());
  });

  $('.NodeAllRoles').click(function() {
    $('.NodeAccessRoles option').each(function(i) {
      if ($(this).val() == '') $('.NodeAccessRoles').empty();
    });
    $('.NodeAllRoles option:selected').appendTo($('.NodeAccessRoles'));
    $('.NodeAccessRoles option').tsort();
  });
  $('.NodeAccessRoles').click(function() {
    if ($(this).val() != '') {
      $('.NodeAccessRoles option:selected').appendTo($('.NodeAllRoles'));
      if ($('.NodeAccessRoles option').size() == 0) {
        $('.NodeAccessRoles').append(MakeOption('Everyone', '', true));
      }
      $('.NodeAllRoles option').tsort();
    }
  });
  $('.NodeAllRoles2').click(function() {
    $('.NodeEditRoles option').each(function(i) {
      if ($(this).val() == '') $('.NodeEditRoles').empty();
    });
    $('.NodeAllRoles2 option:selected').appendTo($('.NodeEditRoles'));
    $('.NodeEditRoles option').tsort();
  });
  $('.NodeEditRoles').click(function() {
    if ($(this).val() != '') {
      $('.NodeEditRoles option:selected').appendTo($('.NodeAllRoles2'));
      if ($('.NodeEditRoles option').size() == 0) {
        $('.NodeEditRoles').append(MakeOption('No one', '', true));
      }
      $('.NodeAllRoles2 option').tsort();
    }
  });
  $('.NodeAllRoles3').click(function() {
    $('.NodeApproveRoles option').each(function(i) {
      if ($(this).val() == '') $('.NodeApproveRoles').empty();
    });
    $('.NodeAllRoles3 option:selected').appendTo($('.NodeApproveRoles'));
    $('.NodeApproveRoles option').tsort();
  });
  $('.NodeApproveRoles').click(function() {
    if ($(this).val() != '') {
      $('.NodeApproveRoles option:selected').appendTo($('.NodeAllRoles3'));
      if ($('.NodeApproveRoles option').size() == 0) {
        $('.NodeApproveRoles').append(MakeOption('Use edit roles', '', true));
      }
      $('.NodeAllRoles3 option').tsort();
    }
  });




  //  $('.treenode').bind("click", function() {

  //    alert('test');

  //    selectedNode = $(this);
  //    $('.selected').removeClass('selected');
  //    selectedNode.addClass('selected');
  //    LoadNodeData($(this).attr('url'));
  //  });
  //  


  //  if ($.browser.msie) {

  //    alert('IE 8 1');

  ////    $('.treenode').bind("click", function() {
  ////      alert('test 1');
  ////    });
  //    $('.treenode').live("click", function(event) {
  //      alert('test 2');
  //    });
  //  }


  //$('.treenode').click(function() {
  //$('.treenode').bind("click", function(event) {
  //$('.treenode').live("click", function(event) {



});

function formJSONNode() {
  var IsAdd = !$('#SiteManagerPopup .NodeUrl').is(':disabled');
  var retval = '{ node: {';
  retval += 'ParentUrl:"' + (IsAdd ? $('.UrlText').text() : "") + '",';
  retval += 'Url:"' + (IsAdd ? $('.UrlText').text() + $('.NodeUrl').val() : $('.NodeUrl').val()) + '",';
  retval += 'Title:"' + $('.NodeTitle').val() + '",';
  retval += 'Description:"' + $('.NodeDescription').val() + '",';
  retval += 'ApprovalEmail:"' + $('.NodeApprovalEmail').val() + '",';
  retval += 'SEOKeywords:"' + $('.NodeSEOKeywords').val() + '",';
  retval += 'SEODescription:"' + $('.NodeSEODescription').val() + '",';
  retval += 'NavItem:"' + $('.NodeNavItem :checkbox').attr('checked') + '",';
  retval += 'UseSSL:"' + $('.NodeUseSSL :checkbox').attr('checked') + '",';
  retval += 'Hidden:"' + $('.NodeHidden :checkbox').attr('checked') + '",';
  retval += 'AllowChildren:"' + $('.NodeAllowChildren :checkbox').attr('checked') + '",';
  retval += 'AccessRoles:"';
  $('.NodeAccessRoles option').each(function() {
    retval += $(this).val() + ',';
  });
  retval += '",';
  retval += 'EditRoles:"';
  $('.NodeEditRoles option').each(function() {
    retval += $(this).val() + ',';
  });
  retval += '",';
  retval += 'ApproveRoles:"';
  $('.NodeApproveRoles option').each(function() {
    retval += $(this).val() + ',';
  });
  retval += '"';
  retval += '}, IsAdd:"' + IsAdd + '", TemplatePath:"' + $('.NodeTemplate').val() + '" }';

  return retval;
};

//-- This method is used to parse the returned server JSON to make sure it is safe to use --//
String.parseJSON = (function(s) {
  var m = {
    '\b': '\\b',
    '\t': '\\t',
    '\n': '\\n',
    '\f': '\\f',
    '\r': '\\r',
    '"': '\\"',
    '\\': '\\\\'
  };
  s.parseJSON = function(filter) {
    try {
      if (/^("(\\.|[^"\\\n\r])*?"|[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t])+?$/.test(this)) {
        var j = eval('(' + this + ')');
        if (typeof filter === 'function') {
          function walk(k, v) {
            if (v && typeof v === 'object') {
              for (var i in v) {
                if (v.hasOwnProperty(i)) {
                  v[i] = walk(i, v[i]);
                }
              }
            }
            return filter(k, v);
          }
          j = walk('', j);
        }
        return j;
      }
    } catch (e) {
    }
    //throw new SyntaxError("parseJSON: filter failed");
    throw new SyntaxError("Trouble loading data please contact support@novelprojects.com");
  };
}
)(String.prototype);
function SyntaxError(e) {
  alert(e);
}