define(['baseView', 'loading', 'emby-input', 'emby-select', 'emby-button', 'emby-checkbox', 'emby-scroller', 'emby-select', 'flexStyles'], function (BaseView, loading) {
    'use strict';

    function View(view, params) {
        BaseView.apply(this, arguments);

        var page = view;
        var instance = this;

        // --- Bookmark buttons ---
        page.querySelector('.btnAdd').addEventListener('click', instance.addStreamPopup.bind(instance));

        page.querySelector('.btnCancel').addEventListener('click', function (e) {
            this.closest('.streamPopup').classList.add('hide');
        });

        page.querySelector('.streamList').addEventListener('click', function (e) {
            var btnDeleteStream = e.target.closest('.btnDeleteStream');
            if (btnDeleteStream) {
                instance.deleteStream(parseInt(btnDeleteStream.getAttribute('data-index')));
            }
        });

        page.querySelector('.streamForm').addEventListener('submit', function (e) {
            e.preventDefault();

            page.querySelector('.streamPopup').classList.add('hide');
            var form = this;

            var newEntry = true;
            var name = page.querySelector('.Name').value;
            var image = page.querySelector('.Image').value;
            var url = page.querySelector('.URL').value;
            var type = page.querySelector('.Type').value;
            var userId = ApiClient.getCurrentUserId();

            if (instance.config.Bookmarks.length > 0) {
                for (var i = 0; i < instance.config.Bookmarks.length; i++) {
                    if (instance.config.Bookmarks[i].Name === name) {
                        newEntry = false;
                        instance.config.Bookmarks[i].Image = image;
                        instance.config.Bookmarks[i].Path = url;
                        instance.config.Bookmarks[i].Protocol = type;
                        instance.config.Bookmarks[i].UserId = userId;
                    }
                }
            }

            if (newEntry) {
                var conf = { Name: name, Image: image, Path: url, Protocol: type, UserId: userId };
                instance.config.Bookmarks.push(conf);
            }

            instance.save();
            instance.populateStreamList();
            return false;
        });

        // --- Playlist buttons ---
        page.querySelector('.btnAddPlaylist').addEventListener('click', instance.addPlaylistPopup.bind(instance));

        page.querySelector('.playlistList').addEventListener('click', function (e) {
            var btnDeletePlaylist = e.target.closest('.btnDeletePlaylist');
            if (btnDeletePlaylist) {
                instance.deletePlaylist(parseInt(btnDeletePlaylist.getAttribute('data-index')));
            }
        });

        page.querySelector('.playlistForm').addEventListener('submit', function (e) {
            e.preventDefault();

            page.querySelector('.playlistPopup').classList.add('hide');

            var name = page.querySelector('.PlaylistName').value;
            var url = page.querySelector('.PlaylistURL').value;
            var userId = ApiClient.getCurrentUserId();

            var newEntry = true;
            if (instance.config.M3UPlaylists.length > 0) {
                for (var i = 0; i < instance.config.M3UPlaylists.length; i++) {
                    if (instance.config.M3UPlaylists[i].Name === name) {
                        newEntry = false;
                        instance.config.M3UPlaylists[i].Path = url;
                        instance.config.M3UPlaylists[i].UserId = userId;
                    }
                }
            }

            if (newEntry) {
                var pl = { Name: name, Path: url, UserId: userId };
                instance.config.M3UPlaylists.push(pl);
            }

            console.log("Playlist being added/updated:", instance.config.M3UPlaylists);

            instance.save();
            instance.populatePlaylistList();
            return false;
        });
    }

    Object.assign(View.prototype, BaseView.prototype);

    // --- Bookmarks ---
    View.prototype.populateStreamList = function () {
        var streams = this.config.Bookmarks;
        var html = "";

        for (var i = 0; i < streams.length; i++) {
            var stream = streams[i];
            html += '<div class="listItem listItem-border">';
            html += '<i class="listItemIcon md-icon">live_tv</i>';
            html += '<div class="listItemBody two-line">';
            html += '<h3 class="listItemBodyText">' + stream.Name + '</h3>';
            html += '</div>';
            html += '<button type="button" is="paper-icon-button-light" class="btnDeleteStream" data-index="' + i + '" title="Delete"><i class="md-icon">delete</i></button>';
            html += '</div>';
        }

        var streamList = this.view.querySelector('.streamList');
        streamList.innerHTML = html;
        streamList.classList.toggle('hide', streams.length === 0);
    };

    View.prototype.deleteStream = function (index) {
        var msg = "Are you sure you wish to delete this bookmark?";
        var instance = this;

        require(['confirm'], function (confirm) {
            confirm(msg, "Delete Bookmark").then(function () {
                instance.config.Bookmarks.splice(index, 1);
                instance.save();
                instance.populateStreamList();
            });
        });
    };

    View.prototype.addStreamPopup = function () {
        var page = this.view;
        page.querySelector('.Name').value = '';
        page.querySelector('.Image').value = '';
        page.querySelector('.URL').value = '';
        page.querySelector('.streamPopup').classList.remove('hide');
        page.querySelector('.Name').focus();
    };

    // --- M3UPlaylists ---
    View.prototype.populatePlaylistList = function () {
        var M3UPlaylists = this.config.M3UPlaylists;
        var html = "";

        for (var i = 0; i < M3UPlaylists.length; i++) {
            var pl = M3UPlaylists[i];
            html += '<div class="listItem listItem-border">';
            html += '<i class="listItemIcon md-icon">queue_music</i>';
            html += '<div class="listItemBody two-line">';
            html += '<h3 class="listItemBodyText">' + pl.Name + '</h3>';
            html += '</div>';
            html += '<button type="button" is="paper-icon-button-light" class="btnDeletePlaylist" data-index="' + i + '" title="Delete"><i class="md-icon">delete</i></button>';
            html += '</div>';
        }

        var playlistList = this.view.querySelector('.playlistList');
        playlistList.innerHTML = html;
        playlistList.classList.toggle('hide', M3UPlaylists.length === 0);
    };

    View.prototype.addPlaylistPopup = function () {
        var page = this.view;
        page.querySelector('.PlaylistName').value = '';
        page.querySelector('.PlaylistURL').value = '';
        page.querySelector('.playlistPopup').classList.remove('hide');
        page.querySelector('.PlaylistName').focus();
    };

    View.prototype.deletePlaylist = function (index) {
        var msg = "Are you sure you wish to delete this playlist?";
        var instance = this;

        require(['confirm'], function (confirm) {
            confirm(msg, "Delete Playlist").then(function () {
                instance.config.M3UPlaylists.splice(index, 1);
                instance.save();
                instance.populatePlaylistList();
            });
        });
    };

    // --- Save ---
    View.prototype.save = function () {
        var instance = this;
        ApiClient.getPluginConfiguration("c333f63b-83e9-48d2-8b9a-c5aba546fb1e").then(function (config) {
            config.Bookmarks = instance.config.Bookmarks;
            config.M3UPlaylists = instance.config.M3UPlaylists;
            console.log("Final config to save:", config);
            ApiClient.updatePluginConfiguration("c333f63b-83e9-48d2-8b9a-c5aba546fb1e", config).then(Dashboard.processPluginConfigurationUpdateResult);
        });
    };

    // --- On Resume ---
    View.prototype.onResume = function (options) {
        BaseView.prototype.onResume.apply(this, arguments);

        var instance = this;
        loading.show();

        ApiClient.getPluginConfiguration("c333f63b-83e9-48d2-8b9a-c5aba546fb1e").then(function (config) {
            instance.config = config;

            if (!instance.config.Bookmarks) instance.config.Bookmarks = [];
            if (!instance.config.M3UPlaylists) instance.config.M3UPlaylists = [];

            instance.populateStreamList();
            instance.populatePlaylistList();
            loading.hide();
        });
    };

    return View;
});
