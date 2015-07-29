var RefreshManager = (function () {

    var _refreshUrl, _isRefreshedUrl, self;
    
    function RefreshManager(refreshUrl, isRefreshedUrl) {

        _refreshUrl = refreshUrl;
        _isRefreshedUrl = isRefreshedUrl;
        self = this;

        this._setupRefreshLinkClikedEvent();
    }
    
    RefreshManager.prototype.toggleReloading = function (reloading) {
        if (reloading) {
            $('#reloadingLabel').show();
            $('#refreshLink').hide();
        }
        else {
            $('#reloadingLabel').hide();
            $('#refreshLink').show();
        }
    };
    
    RefreshManager.prototype.checkRefreshingStatusPeriodically = function () {
        $.ajax({
            url: _isRefreshedUrl
        }).done(function (res) {
            if (res.running) {
                setTimeout(self.checkRefreshingStatusPeriodically, 5000);
            }

            self.toggleReloading(res.running);
            $('#lastReloadTimeLabel').text(res.lastRunTime);
        });
    };

    RefreshManager.prototype._setupRefreshLinkClikedEvent = function () {
        var self = this;
        $('#refreshLink').click(function () {
            $.ajax({
                url: _refreshUrl
            }).done(function () {
                self.toggleReloading(true);
                self.checkRefreshingStatusPeriodically();
            }).fail(function () {
                $("<div>Failed to Refresh</div>").insertAfter("#refreshLink");
            });
        })
    };

    return RefreshManager;
})();