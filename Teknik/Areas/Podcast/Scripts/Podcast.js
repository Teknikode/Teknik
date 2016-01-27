$(document).ready(function () {
    $("#podcast_submit").click(function () {
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
        fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

        var xhr = new XMLHttpRequest();
        xhr.open('POST', addPodcastURL);
        xhr.send(fd);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                obj = JSON.parse(xhr.responseText);
                if (obj.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + obj.error + '</div>');
                }
            }
        }
        return false;
    });

    $('#editPodcast').on('show.bs.modal', function (e) {
        $("#edit_podcast_episode").val("");
        $("#edit_podcast_title").val("");
        $("#edit_podcast_description").val("");
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
    });

    $("#edit_submit").click(function () {
        $('#editPodcast').modal('hide');
        podcastId = $("#edit_podcast_podcastId").val();
        episode = $("#edit_podcast_episode").val();
        title = $("#edit_podcast_title").val();
        description = $("#edit_podcast_description").val();
        $.ajax({
            type: "POST",
            url: editPodcastURL,
            data: AddAntiForgeryToken({ podcastId: podcastId, episode: episode, title: title, description: description }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    $("#comment_submit").click(function () {
        $('#newComment').modal('hide');
        podcastId = $("#podcastId").val();
        post = $("#comment_post").val();
        $.ajax({
            type: "POST",
            url: addCommentURL,
            data: AddAntiForgeryToken({ podcastId: postID, article: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            data: AddAntiForgeryToken({ commentID: postID, article: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            data: AddAntiForgeryToken({ podcastId: podcastId, publish: false }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            data: AddAntiForgeryToken({ podcastId: podcastId, publish: true }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
                    data: AddAntiForgeryToken({ podcastId: podcastId }),
                    success: function (html) {
                        if (html.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                        }
                    }
                });
            }
        });
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
                    data: AddAntiForgeryToken({ commentID: post_id }),
                    success: function (html) {
                        if (html.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                        }
                    }
                });
            }
        });
    });
}