$(document).ready(function () {
    $("#podcast_submit").click(function () {
        $.blockUI({ message: '<div class="text-center"><h3>Saving...</h3></div>' });
        $('#newPodcast').modal('hide');
        var fd = new FormData();
        var fileInput = document.getElementById('podcast_files');
        for (i = 0; i < fileInput.files.length; i++) {
            //Appending each file to FormData object
            fd.append(fileInput.files[i].name, fileInput.files[i]);
        }
        episode = $("#podcast_episode").val();
        title = $("#podcast_title").val();
        description = $("#podcast_description").val();

        fd.append("episode", episode);
        fd.append("title", title);
        fd.append("description", description);

        var xhr = new XMLHttpRequest();
        xhr.open('POST', addPodcastURL);
        xhr.send(fd);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                obj = JSON.parse(xhr.responseText);
                $.unblockUI();
                if (obj.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(obj) + '</div>');
                }
            }
        };
        return false;
    });

    $('#editPodcast').on('show.bs.modal', function (e) {
        $("#edit_podcast_episode").val("");
        $("#edit_podcast_title").val("");
        $("#edit_podcast_description").val("");
        $("#edit_podcast_cur_file_list").val("");
        $("#edit_podcast_cur_files").html('');
        podcastId = $(e.relatedTarget).attr("id");
        $("#edit_podcast_podcastId").val(podcastId);
        $.ajax({
            type: "POST",
            url: getPodcastEpisodeURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    $("#edit_podcast_episode").val(html.result);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: getPodcastTitleURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    $("#edit_podcast_title").val(html.result);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: getPodcastDescriptionURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    $("#edit_podcast_description").val(html.result);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: getPodcastFilesURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    var files = html.result.files
                    var fileList = [];
                    for (var i = 0; i < files.length; i++) {
                        var fileId = files[i].id;
                        var fileName = files[i].name;
                        $("#edit_podcast_cur_files").append(' \
                                                        <div class="alert alert-success uploaded_file_' + fileId + '"> \
                                                            <button type="button" class="close podcast_file_delete" id="' + fileId + '">&times;</button> \
                                                            ' + fileName + ' \
                                                        </div> \
                                                        ');
                        fileList[i] = fileId;
                        linkEditFileDelete('.podcast_file_delete');
                    }
                    $("#edit_podcast_cur_file_list").val(fileList.toString());
                }
            }
        });
    });

    $("#edit_submit").click(function () {
        $.blockUI({ message: '<div class="text-center"><h3>Saving...</h3></div>' });
        $('#editPodcast').modal('hide');
        var fd = new FormData();
        var fileInput = document.getElementById('edit_podcast_files');
        for (i = 0; i < fileInput.files.length; i++) {
            //Appending each file to FormData object
            fd.append(fileInput.files[i].name, fileInput.files[i]);
        }

        podcastId = $("#edit_podcast_podcastId").val();
        episode = $("#edit_podcast_episode").val();
        title = $("#edit_podcast_title").val();
        description = $("#edit_podcast_description").val();
        allFiles = $("#edit_podcast_cur_file_list").val();
        
        fd.append("podcastId", podcastId);
        fd.append("episode", episode);
        fd.append("title", title);
        fd.append("description", description);
        fd.append("fileIds", allFiles);

        var xhr = new XMLHttpRequest();
        xhr.open('POST', editPodcastURL);
        xhr.send(fd);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                obj = JSON.parse(xhr.responseText);
                $.unblockUI();
                if (obj.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(obj) + '</div>');
                }
            }
        };
        return false;
    });

    $("#comment_submit").click(function () {
        $('#newComment').modal('hide');
        podcastId = $("#podcastId").val();
        post = $("#comment_post").val();
        $.ajax({
            type: "POST",
            url: addCommentURL,
            data: { podcastId: postID, article: post },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });

    $('#editComment').on('show.bs.modal', function (e) {
        $("#edit_comment_post").val("");
        commentID = $(e.relatedTarget).attr("id");
        $("#edit_comment_id").val(commentID);
        $.ajax({
            type: "POST",
            url: getCommentArticleURL,
            data: { commentID: commentID },
            success: function (html) {
                if (html.result) {
                    $("#edit_comment_post").val(html.result);
                }
            }
        });
    });

    $("#edit_comment_submit").click(function () {
        $('#editComment').modal('hide');
        postID = $("#edit_comment_id").val();
        post = $("#edit_comment_post").val();
        $.ajax({
            type: "POST",
            url: editCommentURL,
            data: { commentID: postID, article: post },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });
});

function loadMorePodcasts(start, count) {
    podcastId = $(".podcast-main").attr("id");
    $.ajax({
        type: "POST",
        url: getPodcastsURL,
        data: { podcastId: podcastId, count: count, startPodcastID: start },
        success: function (html) {
            if (html) {
                $(".podcast-main").append(html);
                linkPodcastDelete('.delete_podcast');
                linkPodcastPublish('.publish_podcast');
                linkPodcastUnpublish('.unpublish_podcast');
                $(window).bind('scroll', bindScrollPosts);
            }
        }
    });
}

function loadMoreComments(start, count) {
    post_id = $(".podcast-comments").attr("id");
    $.ajax({
        type: "POST",
        url: getCommentsURL,
        data: { postID: post_id, count: count, startCommentID: start },
        success: function (html) {
            if (html) {
                $(".podcast-comments").append(html);
                linkCommentDelete('.delete_comment');
                $(window).bind('scroll', bindScrollComments);
            }
        }
    });
}

function bindScrollPosts() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMorePodcasts(startPodcast, podcasts);
        startPodcast = startPodcast + podcasts;
    }
}

function bindScrollComments() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMoreComments(startComment, comments);
        startComment = startComment + comments;
    }
}

function linkPodcastUnpublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPodcastURL,
            data: { podcastId: podcastId, publish: false },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
}

function linkPodcastPublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPodcastURL,
            data: { podcastId: podcastId, publish: true },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
}

function linkPodcastDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        bootbox.confirm("Are you sure you want to delete the podcast?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deletePodcastURL,
                    data: { podcastId: podcastId },
                    success: function (response) {
                        if (response.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
}

function linkEditFileDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        podFile = object.attr('id');
        allFiles = $("#edit_podcast_cur_file_list").val();
        var fileList = allFiles.split(',');
        var index = fileList.indexOf(podFile);
        if (index != -1) {
            fileList.splice(index, 1);
            $("#edit_podcast_cur_file_list").val(fileList.toString());
        }
        $(".uploaded_file_" + podFile).remove();
        return false;
    });
}

function linkCommentDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = object.attr("id");
        bootbox.confirm("Are you sure you want to delete your comment?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteCommentURL,
                    data: { commentID: post_id },
                    success: function (response) {
                        if (response.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
}
