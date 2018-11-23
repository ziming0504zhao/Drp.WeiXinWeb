$(function(){
	document.body.addEventListener('touchstart', function () { }); //...空函数即可
})
function showTip(msg){
	if($(".verTc").length<1){
		var strVar = "";
	    strVar += "<div class=\"verTc\"><abbr id=\"showBug\">这里是验证提示信息<\/abbr><\/div>\n";
	    $("body").append(strVar);
	}
	$("#showBug").html(msg);
	//表单验证的弹出框JS
	$(".verTc").stop().fadeIn(1500).fadeOut(600);
}
//倒计时
function countDown() {
	$('#getVerify').hide();
	$('#time_change').show();
	$("#time_change").html("还剩60秒");
	$("#time_val").val(60);
	var intervalId = setInterval(function() {
		var time = $("#time_val").val() - 1;
		if (time < 1) {
			$("#time_change").hide();
			$("#getVerify").show();
			clearInterval(intervalId);
			return true;
		}
		$("#time_val").val(time);
		if (time < 10) {
			$("#time_change").html("还剩0" + time + "秒");
		} else {
			$("#time_change").html("还剩" + time + "秒");
		}
	}, 1000);
}
// 校验手机号码
function checkMobile(){
	var m=$.trim($('#mobile').val());
	$("#checkMobile").empty();
	if(!m){
		showTip("请输入11位手机号码")
		return false;
	}else if(!/^[1][0-9]{10}$/.test(m)){
		showTip("手机号码格式错误，请重新输入")
		return false;
	}
	return true;
}
function getObjectURL(file) {
    var url = null ;
    if (window.createObjectURL!=undefined) { // basic
        url = window.createObjectURL(file) ;
    } else if (window.URL!=undefined) { // mozilla(firefox)
        url = window.URL.createObjectURL(file) ;
    } else if (window.webkitURL!=undefined) { // webkit or chrome
        url = window.webkitURL.createObjectURL(file) ;
    }
    return url ;
}

//tab切换
function tab(content){
	var index = $(event.target).index();
	var parentNode = $(event.target).parent();
	console.log($(event.target).index())
	$(".selected").removeClass('selected');
	$(parentNode).children().eq(index).addClass('selected');
	$('.'+content).hide();
	$('.'+content).eq(index).show();
}