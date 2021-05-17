$(document).ready(function(){ 
  $('#npUpload .del').live('click',function(){
    var filename = $(this).parents('.file').find('.name').html();
    getFlashMovie('npMainFlashUpload').removeFile(filename);
  });
  
  $('#npUpload .tl, #npUpload .ts, #npUpload .dts').live('mouseover',function(){
    $(this).find('.message').show();
    $(this).css('z-index',20);
  }).live('mouseout',function(){
    $(this).find('.message').hide();
    $(this).css('z-index',1);
  });
  
  $('#npUpload .uploadbtn').one('click',function(){
    startUpload();
  });
  
  $('#npUpload object').focus();
});

var totalsize;

function clearList(){
  $('#npUpload .files').html('');
  
  totalsize = 0;
  
  $('#npUpload object').blur();
  $('#npUpload .hide').removeClass('hide');
  $('#npUpload .nextstep, #npUpload .step').addClass('hide');
}

function filesTooLarge(names, sizes){
  for(var i=0; i<names.length; i++){
    var filename = $('#npUpload .files .file .name:contains("' + names[i] + '")');
    var file = filename.parents('.file');
    
    if (filename.length == 1){
      file.addClass('tl');
      file.find('.size').css('font-weight','bold');
      
      if (enforceMax) 
        file.addClass('enforce');
      
      if (file.find('.message').length == 0)
        file.find('.details').append('\n\t<div class="message"><span class="topArrow"></span>This file\'s size is too large, please compress your image or select an image with a smaller filesize.</div>');
    }
  }
}

function filesTooSmall(names){
  for(var i=0; i<names.length; i++){
    var filename = $('#npUpload .files .file .name:contains("' + names[i] + '")');
    var file = filename.parents('.file');
    
    if (filename.length == 1){
      file.addClass('ts');
      file.find('.size').css('font-weight','bold');
      
      if (enforceMin)
        file.addClass('enforce');
      
      if (file.find('.message').length == 0)
        file.find('.details').append('\n\t<div class="message"><span class="topArrow"></span>This file\'s size is too small, please select a higher resolution version of your image.</div>');
      else
        file.find('.details .message').append('\n\t<br /><br />Additionally, this file\'s size is too small, please select a higher resolution version of your image. Thank you.');
    }
  }
}

function filesDimTooSmall(names){
  for(var i=0; i<names.length; i++){
    var filename = $('#npUpload .files .file .name:contains("' + names[i] + '")');
    var file = filename.parents('.file');
    
    if (filename.length == 1){
      file.addClass('dts');
      
      if (enforceDim)
        file.addClass('enforce');
      
      if (file.find('.message').length == 0)
        file.find('.details').append('\n\t<div class="message"><span class="topArrow"></span>This file does not meet the recommended minimum image dimensions. If you have a high resolution version of this image, you should upload that so your prints come out as clear as possible. Thank you.</div>');
      else
        file.find('.details .message').append('\n\t<br /><br />Additionally, this file does not meet the recommended minimum image dimensions. If you have a high resolution version of this image, you should upload that so your prints come out as clear as possible. Thank you.');
    }
  }
}

function imageDPITooSmall(names, sizes, dpi){
  filesSelected(names, sizes);
}

function filesSelected(names, sizes){
  for(var i=0; i<names.length; i++){
    totalsize += sizes[i];
    
    var newFile = '\n<div class="file">';
       newFile += '\n\t<div class="progress" title="' + names[i] + '"></div>';
       newFile += '\n\t<div class="details">'
       newFile += '\n\t\t<div class="name">' + names[i] + '</div>';
       newFile += '\n\t\t<div class="size">' + formatSize(sizes[i]) + '</div>';
       newFile += '\n\t\t<div class="delete"><a class="del"><span>delete this file</span></a></div>';
       newFile += '\n\t\t<div class="clear"></div>';
       newFile += '\n\t</div>';
       newFile += '\n</div>';
    
    $('#npUpload .files').append(newFile);
  }
  $('#npUpload .totalfiles .count').html($('#npUpload .files .file:not(.enforce)').length);
  $('#npUpload .totalsize').html(formatSize(totalsize));
  
  if (autoUpload){
    startUpload();
  }
}

function startUpload(){
  $('#npUpload .filetop .delete').text('Status');
  $('#npUpload .files .delete').html('');
  $('#npUpload .files .progress').removeClass('hide');
  $('#npUpload .step, #npUpload .nextstep, #npUpload .uploadbtn, #npUpload .cancel').addClass('hide');
  
  getFlashMovie('npMainFlashUpload').uploadFiles(enforceMax, enforceMin, enforceDim);
}

function formatSize(ufsize){
  var size = Math.round((parseInt(ufsize,10)*10)/1024)/10;
  var sizel;
  
  if (size<1024) sizel = size + ' KB';
  else sizel = Math.round((size*10)/1024)/10 + ' MB';
  
  return sizel;
}
 
function uploadProgress(file_name, bytes_loaded, bytes_total){ 
  var file = $('.files .file:contains("' + file_name + '")');
  var pb = $("#npUpload .files .progress[title='" + file_name + "']");
  var percentage = (bytes_loaded / bytes_total); 
  
  pb.css({
    width:percentage * (file.width()+9),
    borderRight:'1px solid red'
  });
  
  log(file_name + ': ' + (percentage*100) + '%');
}

function eachComplete(file_name){
  var file = $('#npUpload .files .file:contains("' + file_name + '")');
  var pb = $('#npUpload .files .progress[title="' + file_name + '"]');
  
  file.find('.delete').html('<span class="done"></span>');
  pb.remove();
}

function uploadComplete(){
  $('#npUpload .redirect').click();
}
  
function getFlashMovie(movieName) {
  return (jQuery.browser == 'msie' && jQuery.browser.version == '6.0') ? window[movieName] : document[movieName];
  //var isIE = navigator.appName.indexOf("Microsoft") != -1;
  //return (isIE) ? window[movieName] : document[movieName];
}

function httpError(file_name, error, url){
  log(file_name + ' had error ' + error + ' for ' + url);
}
 
function ioError(file_name){
  log(file_name + ' had io error');
}
 
function securityError(file_name, error){
  log(file_name + ' had security error: ' + error);
}

function log(msg){
  try { $('.npUploadResponse').html(msg); }
  catch (err) {}
}