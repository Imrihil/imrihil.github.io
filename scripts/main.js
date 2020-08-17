$(window).scroll(function () {
    glueHeader();
});

$(document).ready(function () {
    setHeaderHeight();
});

$(window).resize(function () {
    setHeaderHeight();
});

function setHeaderHeight() {
    if ($(window).width() > 992) {
        var headerHeight = $('#header').height();
        var windowHeight = $(window).height();
        var bodyHeight = $('body').height();
        console.log(headerHeight);
        $('#header').height(Math.max(headerHeight, windowHeight, bodyHeight));
    }
}

function glueHeader() {
    if ($(window).width() > 992) {
        var photoHeight = $('.header-photo img').outerHeight();
        if ($(this).scrollTop() > photoHeight) {
            $('#header').addClass("position-fixed");
            var marginTopValue = -photoHeight + 'px';
            $('#header').css('margin-top', marginTopValue);
        } else {
            $('#header').removeClass("position-fixed");
            $('#header').css('margin-top', '0');
        }
    }
}