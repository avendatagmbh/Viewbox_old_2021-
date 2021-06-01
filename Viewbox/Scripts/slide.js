$(function() {
    $(".btn-slide").click(function() {
        $("#panel").slideToggle("slow");
        $(this).toggleClass("active");
//        if ($("#panel").css("display") != "none") {
//            $(".page").css("top", "60px");
//        }
//        else {
//            $(".page").css("top", "100px");
//        }
        return false;
    });
});