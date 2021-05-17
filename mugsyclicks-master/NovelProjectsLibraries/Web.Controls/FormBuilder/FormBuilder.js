$(function() {
	LoadValues();
	LoadJquery();
	ShowHide();
	
	if (fid.val()!='00000000-0000-0000-0000-000000000000' && fid.val()!='')
		LoadForm();
	
	var qid,lbl,type,val,req,valid,rows,len,width,dir,col,fid,fname,fsendemail,femail;
  
  function LoadValues()
  {
		qid = $('.QID');
		lbl = $('.QLabel');
		type = $('.QType input');
		val = $('.QValues');
		req = $('.QRequired input');
		valid = $('.QValidationType');
		rows = $('.QRows');
		len = $('.QMaxLength');
		width = $('.QWidth');
		dir = $('.QRepeatDirection input');
		col = $('.QRepeatColumns');
		fid = $('.FormID');
		fname = $('.FormName');
		fsendemail = $('.FormSendEmail');
		femail = $('.FormEmail');
  }

	$('.AddQuestion').click(function(){
		if (type.filter(":checked").val() == 'Check Box List' || type.filter(":checked").val() == 'Check Box') req.filter(":first").attr("checked",true);
		$.ajax({
			type: "POST",
			url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/SaveQuestion",
			data: formJSONQuestion(),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function(msg)
			{
				var object = msg.d;
				if (object==undefined || object==null) object = msg;
        object = object.parseJSON();
				fid.val(object.FormID);
				
				$("#EditQuestionsList").html("");
				BuildEditList(object);
				$(".ConfigTabs").tabs("select", 1);
				ResetEditQuestionClick();
				ResetQuestionForm();
			},
			error: function(xhr, msg)
			{
				alert("fail: " + msg + '\n ' + xhr.responseText);
			}
		});
	});
	
	$(".CancelQuestion").click(function(){
		ResetQuestionForm();
	});
	
	function BuildEditList(object)
	{
		for(var i=0; i<object.Questions.length;i++)
		{
			$("#EditQuestionsList").append("<a id='" + object.Questions[i].ID + "' href='javascript:void(0);' class='Delete' alt='Delete' title='Delete'></a>");
			$("#EditQuestionsList").append("<a id='" + object.Questions[i].ID + "' href='javascript:void(0);' class='MoveDown' alt='Move Down' title='Move Down'></a>");
			$("#EditQuestionsList").append("<a id='" + object.Questions[i].ID + "' href='javascript:void(0);' class='MoveUp' alt='Move Up' title='Move Up'></a>");
			$("#EditQuestionsList").append("<a id='" + object.Questions[i].ID + "' href='javascript:void(0);' class='LoadQuestion Edit' title='Edit Question'></a>");
			$("#EditQuestionsList").append(object.Questions[i].Label);
			$("#EditQuestionsList").append("<br/><br/>");
		}
	};
	
	function ResetEditQuestionClick()
	{
		$('.LoadQuestion').click(function(){
			var id = $(this).attr('id');
			$.ajax({
				type: "POST",
				url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/LoadQuestion",
				data: "{ 'ID':'"+ id +"'}",
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				success: function(msg)
				{
					var object = msg.d;
					if (object==undefined || object==null) object = msg;
					object = object.parseJSON();
					
					$('#AddForm strong').text('Edit Question:');
					
					lbl.val(object.Label);
					qid.val(object.ID);
					type.filter("[value='" + object.Type + "']").attr("checked",true);
					val.val(object.Values.replace(/\|/g,"\n"));
					req.filter("[value='" + object.Required + "']").attr("checked",true);
					valid.val(object.ValidationType);
					rows.val(object.Rows);
					len.val(object.MaxLength);
					width.val(object.Width);
					dir.filter("[value='" + object.RepeatDirection + "']").attr("checked",true);
					col.val(object.RepeatColumns);
					
					ShowHide();
					$('.ConfigTabs').tabs("select", 0);
				},
				error: function(xhr, msg)
				{
					alert("fail: " + msg + '\n ' + xhr.responseText);
				}
			});
		});
		$('.MoveUp').click(function(){
			var id = $(this).attr('id');
			SwapQuestions(id, "MoveUp");
		});
		$('.MoveDown').click(function(){
			var id = $(this).attr('id');
			SwapQuestions(id, "MoveDown");
		});
		$('.Delete').click(function(){
			var answer = confirm("Are you sure you want to delete this question?");
			if(answer)
			{
				var id = $(this).attr('id');
				SwapQuestions(id, "Delete");
			}
		});
	}
		
	function SwapQuestions(id, MoveCommand){
		$.ajax({
			type: "POST",
			url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/SwapQuestions",
			data: "{ 'ID':'"+ id +"','MoveCommand':'" + MoveCommand + "'}",
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function(msg)
			{
				var object = msg.d;
				if (object==undefined || object==null) object = msg;
				object = object.parseJSON();
				
				$("#EditQuestionsList").html("");
				BuildEditList(object);
				ResetEditQuestionClick();
				ResetQuestionForm();
			},
			error: function(xhr, msg)
			{
				alert("fail: " + msg + '\n ' + xhr.responseText);
			}
		});
	};
		
	function LoadForm(){
		$.ajax({
			type: "POST",
			url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/LoadForm",
			data: "{ 'FormID':'"+ fid.val() + "'}",
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function(msg)
			{
				var object = msg.d;
				if (object==undefined || object==null) object = msg;
				object = object.parseJSON();
				
				fname.val(object.FormName);
				fsendemail.attr("checked",object.FormSendEmail);
				femail.val(object.FormEmail);
				ShowHide();
				
				$("#EditQuestionsList").html("");
				BuildEditList(object);
				ResetEditQuestionClick();
				ResetQuestionForm();
			},
			error: function(xhr, msg)
			{
				alert("fail: " + msg + '\n ' + xhr.responseText);
			}
		});
	};

	$('.SaveForm').click(function(){
		$.ajax({
			type: "POST",
			url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/SaveForm",
			data: formJSONForm($(this).attr('title')=="Save Form",$(this).attr('title')=="Save For Later"),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function(msg)
			{
				ResetForm();
				ResetQuestionForm();
			},
			error: function(xhr, msg)
			{
				alert("fail: " + msg + '\n ' + xhr.responseText);
			}
		});
	});
	
	$(".ClearForm").click(function(){
		var answer = confirm("Are you sure you want to delete this form?");
		if(answer)
		{
			$.ajax({
				type: "POST",
				url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/ClearForm",
				data: "{'ID':'" + fid.val() + "'}",
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				success: function(msg)
				{
					ResetForm();
					ResetQuestionForm();
				},
				error: function(xhr, msg)
				{
					alert("fail: " + msg + '\n ' + xhr.responseText);
				}
			});
		}
	});
	
	function formJSONQuestion(){
		var retval = '{ Ques: {';
		retval += ' ID:"' + qid.val() + '",';
		retval += ' FormID:"' + fid.val() + '",';
		retval += ' Label:"' + lbl.val() + '",';
		retval += ' Type:"' + type.filter(":checked").val() + '",';
		retval += ' Values:"' + val.val().replace(/\n/g,"|") + '",';
		retval += ' Required:"' + req.filter(":checked").val() + '",';
		retval += ' ValidationType:"' + valid.val() + '",';
		retval += ' Rows:"' + rows.val() + '",';
		retval += ' MaxLength:"' + len.val() + '",';
		retval += ' Width:"' + width.val() + '",';
		retval += ' RepeatDirection:"' + dir.filter(":checked").val() + '",';
		retval += ' RepeatColumns:"' + col.val() + '"';
		retval += '} }';
		
		return retval;
	}
	
	function formJSONForm(visible, saved){
		var retval = '{ Form: {';
		retval += ' ID:"' + fid.val() + '",';
		retval += ' Name:"' + fname.val() + '",';
		retval += ' SendEmail:"' + fsendemail.attr("checked") + '",';
		retval += ' Email:"' + femail.val() + '",';
		retval += ' Visible:"' + visible + '",';
		retval += ' Saved:"' + saved + '"';
		retval += '} }';
		
		return retval;
	}
	
	function ResetQuestionForm()
	{	
		qid.val("00000000-0000-0000-0000-000000000000");
		lbl.val("");
		type.filter(':first').attr("checked",true);
		val.val("");
		req.filter(':first').attr("checked",true);
		$('#QValidationType').attr("selectedIndex","0");
		rows.val("1");
		len.val("100");
		width.val("140");
		dir.filter(':first').attr("checked",true);
		col.val("1");
		$('#AddForm strong').text('Add a New Question:');
		
		ShowHide();
	}
	
	function ResetForm()
	{
		$("#EditQuestionsList").html("");
		fid.val("00000000-0000-0000-0000-000000000000");
		fname.val("New Form");
		fsendemail.attr("checked",false);
		femail.val("");
	}

	function LoadJquery(){
		$('.ConfigTabs').tabs().tabs("select", 2);
		
		$(type).click(function(){
			ShowHide();
		});
		$(req).click(function(){
			ShowHide();
		});
		$(fsendemail).click(function() {
			ShowHide();
		});
	};

	function ShowHide(){
		$('.QText').hide(); $('.QRadio').hide(); $('.QValuesRow').hide(); $('.QRequiredType').hide(); $('.QRequiredRow').show();
		var t = type.filter(":checked").val();
		var r = req.filter(":checked").val();
		if (t == 'Text Field'){
			$('.QText').show();
			if (r == 'True') $('.QRequiredType').show();
		}
		if (t == 'Check Box List' || t == 'Radio Button List') $('.QRadio').show();
		if (t == 'Check Box List' || t == 'Check Box') $('.QRequiredRow').hide();
		if (t == 'Radio Button List' || t == 'Check Box List' || t == 'List Box' || t == 'Drop Down List') $('.QValuesRow').show();
		if ($('.FormSendEmail input:checked').val()!=null) $('.FormSendEmailRow').show();
		else $('.FormSendEmailRow').hide();
	};
});

//-- This method is used to parse the returned server JSON to make sure it is safe to use --//
String.parseJSON  = (function (s) {
  var m = {
    '\b': '\\b',
    '\t': '\\t',
    '\n': '\\n',
    '\f': '\\f',
    '\r': '\\r',
    '"' : '\\"',
    '\\': '\\\\'
  };
  s.parseJSON = function (filter) {
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
) (String.prototype);
function SyntaxError(e)
{
 alert(e);
}