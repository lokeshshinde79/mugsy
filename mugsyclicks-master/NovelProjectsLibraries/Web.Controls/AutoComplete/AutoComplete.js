$(function(){
	AutoComplete();
});

function AutoComplete()
{
	$('.TxtAutoComplete:not(.ac_input)').each(function(i){
		$(this).autocomplete("NovelProjects.Web.AutoComplete.AutoCompleteService.asmx/Search",{
			minChars:$(this).attr('minchars'),
			cacheLength:1,
			selectFirst:false,
			scrollHeight:200,
			delay: $(this).attr('delay'),
			autoFill: $(this).attr('autofill') == "True",
			extraParams:{Table:$(this).attr('table'),Display:$(this).attr('display'),Search:$(this).attr('search'),
				ConnString:$(this).attr('connstring'),ApplicationId:$(this).attr('applicationid'),NoResultsMessage: $(this).attr('noresultsmessage')}
		});
	});
}