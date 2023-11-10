
const ops = {
    cutDirectory: 0,
    copyDirectory: 1,
    cutFile: 2,
    copyFile: 3,
    paste: 4,
    move: 5,
    none: 6
};

var operation = ops.none;
var item;
var editFile = false;
var uploaddirectory;

$('#DirectoryHierarchy').contextMenu({
    selector: 'span',
    events: {
        show: function (options) {
            this.addClass('currently-showing-menu');
        },
        hide: function (options) {
            this.removeClass('currently-showing-menu');
        }
    },
    callback: function (key, options) {
        $(".currently-showing-menu").removeClass("currently-showing-menu");
        switch (key) {
            case "newFolder":
                var name = prompt("New Name:", $(this).text());
                if (name != null && name.trim() != "") {
                    var res = createNew($(this).data("id"), name.trim(), false);
                    if (res != null) {
                        var itm = `<li><span class="directoryHasSub" onclick="DirectoryToggle(this)" data-id="` + res.trim() + `">` + name.trim() + `</span><ul class="directoryContent"></ul></li>`;
                        $(this).next().prepend(itm);
                    }
                }
                break;
            case "CreateFile":
                var name = prompt("New Name:", $(this).text());
                if (name != null && name.trim() != "") {
                    if (name.indexOf('.') > 0) {
                        var res = createNew($(this).data("id"), name, true)
                        if (res != null) {
                            var itm = `<li class="file" data-id="` + res.trim() + `"  onclick="GetContent($(this))">` + name.trim() + `</li>`;
                            $(this).next().prepend(itm);
                        }
                    } else {
                        alert("Please enter file extention!")
                    }
                }
                break;
            case "UploadFile":
                UploadFiles($(this));
                break;
            case "rename":
                var name = prompt('New name of "' + $(this).text() + '"', $(this).text());
                if (name != null) {
                    if (name.trim() != $(this).text()) {
                        if (Rename($(this).data("id"), name, false)) {
                            $(this).text(name);
                        }
                    }
                }
                break;
            case "cut":
                $(".cutted").removeClass("cutted");
                operation = ops.cutDirectory;
                $(this).parent().addClass("cutted");
                item = $(this).parent();
                addAlert("Cut '" + $(this).text() + "'");
                break;
            case "copy":
                $(".cutted").removeClass("cutted");
                operation = ops.copyDirectory;
                $(this).parent().addClass("cutted");
                item = $(this).parent().clone(true);
                addAlert("Cupy '" + $(this).text() + "'");
                break;
            case "paste":
                $(".cutted").removeClass("cutted");
                item.removeClass("cutted");
                if (!$(this).parent().hasClass("cutted") && $(this).parent().find('.cutted').length == 0) {
                    switch (operation) {
                        case ops.cutDirectory:
                            var newParentId = $(this).data("id");
                            var directoryId = item.children().first().data("id");
                            if (MoveDirectory(directoryId, newParentId))
                                $(this).next().prepend(item);
                            break;
                        case ops.copyDirectory:
                            var newParentId = $(this).data("id");
                            var directoryId = item.children().first().data("id");
                            if (MoveDirectory(directoryId, newParentId, true))
                                $(this).next().prepend(item);
                            break;
                        case ops.cutFile:
                            var newParentId = $(this).data("id");
                            var fileId = item.data("id");
                            if (MoveFile(fileId, newParentId))
                                $(this).next().append(item);
                            break;
                        case ops.copyFile:
                            var newParentId = $(this).data("id");
                            var fileId = item.data("id");
                            if (MoveFile(fileId, newParentId, true))
                                $(this).next().append(item);
                            break;
                        default:
                            break;
                    }
                }
                operation = ops.none;
                item = undefined;
                break;
            case "delete":
                if (confirm('Are you sure you want to delete the "' + $(this).text() + '" directory? ')) {
                    Remove($(this).data("id"), false)
                    $(this).parent().remove();
                }
                break;
            default:
                break;
        }
        $(".context-menu-active").removeClass("context-menu-active");
    },
    items: {
        "rename": { name: "Rename", icon: "edit" },
        "cut": { name: "Cut", icon: "cut", disabled: function () { return $(this).parent().parent().get(0).id == 'DirectoryHierarchy'; } },
        "copy": { name: "Copy", icon: "copy", disabled: function () { return $(this).parent().parent().get(0).id == 'DirectoryHierarchy'; } },
        "paste": { name: "Paste", icon: "paste", disabled: function () { return operation > ops.copyFile; } },
        "delete": { name: "Delete", icon: "delete", disabled: function () { return $(this).parent().parent().get(0).id == 'DirectoryHierarchy'; } },
        "add": {
            name: "Add",
            items: {
                newFolder: { name: "New Folder", },
                newFile: {
                    name: "New File",
                    items: {
                        CreateFile: { name: "Create", },
                        UploadFile: { name: "Upload", }
                    }
                },
            }
        },
        //"sep1": "---------",
        //"quit": { name: "Quit", icon: function ($element, key, item) { return 'context-menu-icon context-menu-icon-quit'; } }
    }
});
$.contextMenu({
    selector: '.file',
    events: {
        show: function (options) {
            this.addClass('currently-showing-menu');
        },
        hide: function (options) {
            this.removeClass('currently-showing-menu');
        }
    },
    callback: function (key, options) {
        $(".currently-showing-menu").removeClass("currently-showing-menu");
        switch (key) {
            case "edit":
                GetContent($(this), true)
                break;
            case "rename":
                var name = prompt('New name of "' + $(this).text() + '"', $(this).text());
                if (name != null) {
                    if (name.trim() != $(this).text()) {
                        if (Rename($(this).data("id"), name, true)) {
                            $(this).text(name);
                        }
                    }
                }
                break;
            case "cut":
                $(".cutted").removeClass("cutted");
                operation = ops.cutFile;
                $(this).addClass("cutted");
                item = $(this);
                addAlert("Cut '" + $(this).text() + "'");
                break;
            case "copy":
                $(".cutted").removeClass("cutted");
                operation = ops.copyFile;
                $(this).addClass("cutted");
                item = $(this).clone(true);
                addAlert("Copy '" + $(this).text() + "'");
                break;
            case "delete":
                if (confirm('Are you sure you want to delete the "' + $(this).text() + '" directory? ')) {
                    Remove($(this).data("id"), true)
                    $(this).remove();
                }
                break;
            default:
                break;
        }
    },
    items: {
        "rename": { name: "Rename", icon: "edit" },
        "edit": { name: "Edit", icon: "edit", disabled: function () { return $(this).data("type") != 1 } },
        "cut": { name: "Cut", icon: "cut" },
        "copy": { name: "Copy", icon: "copy" },
        "delete": { name: "Delete", icon: "delete" },
    }
});
GetContent = function (file, edit = false) {
    var id = file.data("id");
    if (editFile) {
        if (!confirm('Are you sure you want to cancel the edit file')) {
            return;
        }
        editFile = false;
    }
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
                        if (edit) {
                            editFile = true;
                            $("#header").append("<div class='col-10'> <h4>" + data.fullPath + "</h4></div><div class='col-2 text-end' > <button id='clipboardbutton' onclick='Save(" + id + ");' class='btn btn-dark'>Save</button></div></div>");
                            $("#content").append(`<textarea id="editContent" style="min-width: 100%;min-height: 100%" is="highlighted-code" language="` + data.contentType + `" tab-size="2" autosize>` + data.content.trim() + `</textarea>`);
                        } else {
                            $("#header").append("<div class='col-10'> <h4>" + data.fullPath + "</h4></div><div class='col-2 text-end' > <button id='clipboardbutton' onclick='Clipboardbutton();' class='btn btn-dark'>Copy Content</button></div></div>");
                            $("#content").append("<pre><code id='clipboardcontainer' class='language-" + data.contentType + "' style='height:550px; overflow: auto; cursor: text;'></code></pre>");
                            $("code").text(data.content.trim());
                            hljs.highlightAll();
                        }
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
function MoveDirectory(directoryId, newParentId, asCopy = false) {
    var url = asCopy ?
        "/api/File/CopyDirectory?directoryId=" :
        "/api/File/MoveDirectory?directoryId=";
    var res = false;
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Moving.. Please Wait..');
    $.ajax({
        async: false,
        type: "POST",
        url: url + directoryId + "&newParentId=" + newParentId,
        data: null,
        dataType: 'text',
        success: function (data) {
            res = true;
            addAlert('Moved Successfully');
        },
        error: function (data) {
            addAlert('Move Error');
        },
    });
    return res;
};
function Remove(Id, isFile) {
    var url = isFile ?
        "/api/File/RemoveFile?fileId=" :
        "/api/File/RemoveDirectory?directoryId=";
    var res = false;
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Removing.. Please Wait..');
    $.ajax({
        async: false,
        type: "POST",
        url: url + Id,
        data: null,
        dataType: 'text',
        success: function (data) {
            res = true;
            addAlert('Removed Successfully');
        },
        error: function (data) {
            addAlert('Remove Error');
        },
    });
    return res;
};
function MoveFile(fileId, newDirectoryId, asCopy = false) {
    var url = asCopy ?
        "/api/File/CopyFile?fileId=" :
        "/api/File/MoveFile?fileId=";
    var res = false;
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Moving.. Please Wait..');
    $.ajax({
        async: false,
        type: "POST",
        url: url + fileId + "&newDirectoryId=" + newDirectoryId,
        data: null,
        dataType: 'text',
        success: function (data) {
            res = true;
            addAlert('Moved Successfully');
        },
        error: function (data) {
            addAlert('Move Error');
        },
    });
    return res;
};
function Rename(Id, name, isFile) {
    var url = isFile ?
        "/api/File/RenameFile?fileId=" :
        "/api/File/RenameDirectory?directoryId=";
    var res = false;
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Name Changing.. Please Wait..');
    $.ajax({
        async: false,
        type: "POST",
        url: url + Id + "&name=" + name,
        data: null,
        dataType: 'text',
        success: function (data) {
            res = true;
            addAlert('Name Changed Successfully');
        },
        error: function (data) {
            addAlert('Rename Error');
        },
    });
    return res;
};
function createNew(Id, name, isFile) {
    var url = isFile ?
        "/api/File/CreateFile?directoryId=" :
        "/api/File/CreateDirectory?directoryId=";
    var res = null;
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Creating.. Please Wait..');
    $.ajax({
        async: false,
        type: "POST",
        url: url + Id + "&name=" + name,
        data: null,
        dataType: 'text',
        success: function (data) {
            res = data;
            addAlert('Created Successfully');
        },
        error: function (data) {
            addAlert('Create Error');
        },
    });
    return res;
};
$("#FileUpload1").change(function () {
    if (window.FormData !== undefined) {
        var fileUpload = $("#FileUpload1").get(0);
        var files = fileUpload.files;
        for (var i = 0; i < files.length; i++) {
            var fileData = new FormData();

            fileData.append("directoryId", uploaddirectory.data("id"));
            fileData.append("fileToUpload", files[i]);
            $.ajax({
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();

                    xhr.upload.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total;
                            percentComplete = parseInt(percentComplete * 100);
                            console.log(percentComplete);

                            if (percentComplete === 100) {

                            }

                        }
                    }, false);

                    return xhr;
                },
                url: '/api/File/UploadFile',
                type: "POST",
                async: false,
                contentType: false, // Not to set any content header
                processData: false, // Not to process data
                data: fileData,
                success: function (res) {
                    var itm = `<li class="file" data-id="` + res.trim() + `"  onclick="GetContent($(this))">` + files[i].name.trim() + `</li>`;
                    uploaddirectory.next().append(itm);
                    // alert(result);
                },
                error: function (err) {
                    alert(files[i].name + " " + err.statusText);
                }
            });

        }
        alert("Complated.");
    } else {
        alert("FormData is not supported.");
    }
    uploaddirectoryId = null;
});
function UploadFiles(dir) {
    uploaddirectory = dir;
    $("#FileUpload1").val(null);
    $("#FileUpload1").click();

};
function Save(id) {
    $("#Search").empty();
    addAlert('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving.. Please Wait..');
    var mdata = {};
    mdata.FileId = id;
    mdata.Text = $("#editContent").val();
    $.ajax({
        type: "POST",
        url: "/api/File/SaveFile",
        data: mdata,
        dataType: 'Text',
        success: function (data) {
            addAlert("Save File Completed.");
        },
        error: function (data) {
            addAlert(data);
        },
    });

};