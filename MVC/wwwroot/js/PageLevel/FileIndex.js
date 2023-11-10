function DirectoryToggle(dir) {
    dir.parentElement.querySelector(".directoryContent").classList.toggle("active");
    dir.classList.toggle("directory-down");
}
$(".directoryHasSub").click(function () {
    DirectoryToggle(this);
});
$(".file").click(function () {
    $(".file").removeClass("active");
    $(this).addClass("active");
    GetContent($(this));
});
GetContent = function (file) {
    var id = file.data("id");
    $(".file").removeClass("active");
    file.addClass("active");
    $.ajax({
        type: "POST",
        url: "/File/GetContent?Id=" + id,
        data: null,
        dataType: 'Json',
        success: function (data) {
            if (data) {
                $("#content").empty();
                $("#header").empty();
                switch (data.fileType) {
                    case 1://Text
                        $("#header").append("<div class='col-10'> <h4>" + data.fullPath + "</h4></div><div class='col-2 text-end' > <button id='clipboardbutton' onclick='Clipboardbutton();' class='btn btn-dark'>Copy Content</button></div></div>");
                        $("#content").append("<pre><code id='clipboardcontainer' class='language-" + data.contentType + "' style='height:550px; overflow: auto; cursor: text;'></code></pre>");
                        $("code").text(data.content.trim());
                        hljs.highlightAll();
                        break;
                    case 2:// Image
                        $("#header").append(" <h4>" + data.fullPath + "</h4>");
                        $("#content").append("<img src='" + data.content + "' class='img-fluid' />");
                        break;
                    default:
                        $("#header").append("<div class='col-10'> <h4>" + data.fullPath + "</h4></div><div class='col-2 text-end' ><a class='btn btn-dark' href='" + data.content + "'>Download " + data.fileName + " </button></div></div>");
                        var dt = new Date(data.modifyDate);
                        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
                        var scr = '<dl class="row"><dt class="col-sm-2">FileName</dt><dd class="col-sm-10">' +
                            data.fileName + '</dd><dt class="col-sm-2">Path</dt><dd class="col-sm-10">' +
                            data.fullPath + '</dd><dt class="col-sm-2">ModifyDate</dt><dd class="col-sm-10">' +
                            dt.toLocaleDateString("en-US", options) + '</dd><dt class="col-sm-2">ContentType</dt><dd class="col-sm-10">' +
                            data.contentType + '</dd></dl>';
                        //dt.toLocaleDateString(undefined, options)
                        $("#content").append(scr);
                        break;
                }
            }
        },
        error: function (data) {
            if (data) {
                addAlert(data);
            }
        },
    });

};
function Clipboardbutton() {
    copyToClipboard("clipboardcontainer", "clipboardbutton", "Copied!");
};
function addAlert(message) {
    try {
        $(".alert").alert('close');
    } catch (e) {

    }
    $('#alerts').append(
        '<div class="alert alert-warning alert-dismissible fade show" role="alert">' + message + '</div>');
}
$("#searchText").on("keyup", function () {
    var text = $('#searchText').val();
    if (text.trim().length == 0) {
        $(".alert").alert('close');
        $("#Search").hide();
        $("#Hierarchy").show();
    }
});
$("#searchText").change(function () {
    Search();
});
function Search() {
    var text = $('#searchText').val();
    if (text.length > 0) {
        $("#Search").empty();
        addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Searching.. Please Wait..');
        $.ajax({
            type: "POST",
            url: "/File/Search?Text=" + text,
            data: null,
            dataType: 'Json',
            success: function (data) {
                $(".alert").alert('close');
                if (data.length > 0) {
                    $("#Search").show();
                    $("#Hierarchy").hide();
                    $("#Search").empty();
                    jQuery.each(data, function (index, item) {
                        var scr = '<li><span class="directoryHasSub directory-down">' + item.fullPath.replace(item.fileName, "") +
                            '</span><ul class="directoryContent active"  onclick="DirectoryToggle(($(this))"><li class="file' + // file' + item.id +
                            '" data-id="' + item.id + '" data-type="' + item.fileType + '"  onclick="GetContent($(this))">' + item.fileName +
                            '</li></ul></li>';
                        $("#Search").append(scr);
                    });
                } else {
                    $("#Search").hide();
                    $("#Hierarchy").show();
                    addAlert('"' + text + '" Not Found! ');
                }
            },
            error: function (data) {
                if (data) {
                    addAlert(data);
                }
            },
        });
    } else {
        $("#Search").hide();
        $("#Hierarchy").show();
    }
};
