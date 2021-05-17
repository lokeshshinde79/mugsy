$(function() {

	$('.SaveSubmission').click(function(){
		if (Page_ClientValidate("DynamicForm"))
		{
			$.ajax({
				type: "POST",
				url: "NovelProjects.Web.FormBuilder.FormBuilderService.asmx/SaveSubmission",
				data: formJSONSubmission(),
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				success: function(msg)
				{
					$('.DynamicForm').hide();
					$('#ThankYou').show();
				},
				error: function(xhr, msg)
				{
					alert("fail: " + msg + '\n ' + xhr.responseText);
				}
			});
		}
	});

	$('.ResetForm').click(function(){
		location.href = location.href;
	});
	
	function formJSONSubmission(){
		var retval = '{ Form: {';
		retval += ' FormID:"' + $('span[@ID="FormID"]').text() + '", Answers: [ ';
		
		$('.DynamicForm :input').each(function(){
			var valtype = $(this).attr('type');
			var quesid = $(this).attr('id');
			var $name = $('input[@name="' + $(this).attr('name') + '"]');
			switch(valtype)
			{
				case "select-one": //dropdown,listbox
				case "textarea":
				case "text":
					retval += '{ QuestionID:"' + quesid + '",Value:"' + $(this).val() + '" },';
					break;
				case "radio":
					if (retval.indexOf($(this).attr('name'))==-1)
						retval += '{ QuestionID:"' + $(this).attr('name') + '",Value:"' + $name.filter(":checked").val() + '" },';
					break;
				case "checkbox": //checkbox,checkboxlist
					if ($(this).attr('name').indexOf("$")>-1) //means it's a checkboxlist
					{
						if ($name.attr("checked"))
							retval += '{ QuestionID:"' + $(this).attr('name').replace(/\$.*/,"") + '",Value:"' + $(this).next().text() + '" },';
						else
							retval += '{ QuestionID:"' + $(this).attr('name').replace(/\$.*/,"") + '",Value:"" },';
					}
					else
					{
						retval += '{ QuestionID:"' + $(this).attr('name') + '",Value:"' + $(this).attr("checked") + '" },';
					}
					break;
			}
		});
		retval = retval.substring(0,retval.length-1);
		
		retval += '] } }';
		
		return retval;
	}
});