var lastMessageId = "";
var currentThemeName = "main";
try {
    $(document).ready(function () {
        var interval = 1500;
        var refresh = function () {
            //$.ajax({
            //    url: "/statusbar.htm",
            //    cache: false,
            //    success: function (html) { $("#statusbar").html(html); }
            //});
            try {
                $.ajax({
                    url: "/settings.json",
                    cache: false,
                    success: function (json) {
                        if (!(json instanceof Object))
                            json = JSON.parse(json);

                        var themeName = json.ThemeName.toLowerCase();
                        if (currentThemeName != themeName) {
                            $('#theme').attr('href', 'css/' + themeName + '.css');
                            currentThemeName = themeName;
                        }
                    },
                    timeout: 59000
                });
                $.ajax({
                    url: "/messages.json" + (lastMessageId == "" ? "" : "?id=" + lastMessageId),
                    cache: false,
                    success: function (json) {
                        console.log(json);
                        if (!(json instanceof Array))
                            json = JSON.parse(json);

                        var transform = {
                            "tag": "div", "children": [
                            { "tag": "img", "src": "${ChatIconURL}", "html": "" },
                            { "tag": "div", "html": "${Channel}" },
                            { "tag": "div", "html": "${FromUserName}" },
                            { "tag": "div", "html": "${TimeStamp}" },
                            { "tag": "div", "html": "${Text}" },
                            ]
                        };

                        for (var i = 0; i < json.length; i++) {
                            var element = $('#chat').json2html(json[i], transform, { prepend: true });
                            lastMessageId = json[i].Id;
                        }
                        $('#chat > div:hidden').fadeIn(500);

                        while ($('#chat > div').length > 300)
                            $('#chat > div:last-child').remove();

                        setTimeout(function () {
                            refresh();
                        }, interval);
                    },
                    timeout: 59000,
                    error: function () {
                        setTimeout(function () {
                            refresh();
                        }, interval);
                    }

                });

            }
            catch (e) {
            }

        };
        refresh();
        //var x = $.cookie('statusbar.x');
        //var y = $.cookie('statusbar.y');

        //if (x != null && y != null) {
        //    $("#statusbar").offset({ top: y, left: x });
        //}
        //$("#statusbar").draggable({
        //    stop: function () {
        //        $.cookie('statusbar.x', $("#statusbar").offset().left);
        //        $.cookie('statusbar.y', $("#statusbar").offset().top);
        //    },
        //    containment: "parent"
        //});

        $("#chat").animate({ "scrollTop": $("#chat").scrollTop() + 100 });
    });
}
catch (e) {
}

