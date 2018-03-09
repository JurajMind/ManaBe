
var hub = $.connection.smokeSessionHub;
// register
Vue.component('hookah-componet',
    {
        props: ['item', 'url', 'now'],

        template:
            `<div class="col-md-4 col-lg-3 hookah" v-bind:class="{Online: item.Online , Offline : !item.Online , Service : item.Service}"  v-on:click="item.Service = false">
                        <h2><a :href="url + item.Key" >{{ item.Name }}</a></h2>
    <stats-componet v-bind:stats="item.DynamicSmokeStatistic" v-bind:item="item" v-bind:now="now"></stats-componet>
                </div > `
    });


Vue.component('hookah-componet-public',
    {
        props: ['item', 'url', 'now'],

        template:
            `<div class="col-md-4 col-lg-3 hookah" v-bind:class="{ Offline : !item.Online }" >
            <div class="row">
                <div class="col-sm-6"><img class="hookahPicture" v-bind:src="item.HookahPicture""/></div>
                    <div class="col-sm-6">
                        <stats-componet v-bind:stats="item.DynamicSmokeStatistic" v-bind:item="item" v-bind:now="now">
                        </stats-componet>
                    </div>
                </div>
            </div > `
    });



Vue.component('stats-componet',
    {
        props: ['stats', 'item', 'now'],
        computed: {
            // a computed getter
            sessionDuration: function() {
                // `this` points to the vm instance
                var start = moment.utc(this.stats.Start, 'DD-MM-YYYY HH:mm').local();

                if (start.year() === 1)
                    return "";

                var span = moment.duration(this.now.diff(start));
                var timeFormat = span.format('HH:mm:ss');

                return timeFormat;
            },
            lastPuf: function() {
                // `this` points to the vm instance
                var lastPuf = moment.utc(this.stats.LastPufTime, 'DD-MM-YYYY HH:mm:ss').local();

                if (lastPuf.year() === 1)
                    return "";
                return lastPuf.from(this.now);
            }
        },
        template: `
    <table class="table table-responsive" >
    <tbody>
        <tr>
            <td><strong># of pufs</strong></td>
            <td class="pufCount">
                {{ stats.PufCount }} /   {{ item.EstPufCount - stats.PufCount }}
            </td>
        </tr>
        <tr>
            <td><strong><i class="fa fa-clock-o"></i>Smoking time</strong></td>
            <td><span class="smokeDuration">
                {{ stats.SmokeDuration }}
            </span></td>
        </tr>
        <tr>
            <td><strong><i class="fa fa-battery-half"></i>Last puf</strong></td>
            <td><span class="startTime">
                {{ lastPuf }}
            </span></td>
        </tr>
        <tr>
            <td><strong>Session duration</strong></td>
            <td><span class="smokeSessionDuration">{{ sessionDuration }}</span></td>
        </tr>
    </tbody>
    </table > `
    });





var v = new Vue({
    el: '#app',
    data: {
        hookah:[],
        url: url,
        now: moment(),
        place : null,
    },
    created: function() {
        var self = this;

        $.getJSON('/Places/DashBoardData/' + placeId,
            function (json) {
                self.hookah = json.Hookah;
                self.place = json.Place;
            });

        setInterval(function() {
                self.$data.now = moment();
            },
            1000);

        setInterval(function() {
                self.checkOnline();
            },
            100000);

        $.connection.hub.start()
            .done(function() {
                hub.server.joinLounge(placeId);
            });


        $.connection.hub.disconnected(function() {
            console.log("disconected");
            setTimeout(function() {
                    $.connection.hub.start();
                    console.log("reconect");
                },
                5000); // Re-start connection after 5 seconds
        });
    },
    computed: {
        hookahOrderedByPuf:function() {
            return _.orderBy(this.hookah,function(h) {
                return h.DynamicSmokeStatistic.PufCount;
            });
        }
    },
    methods: {

        switchState : function(code, state) {
            var index = this.$data.hookah.map(function (e) { return e.Key; }).indexOf(code);
            if (index === -1)
                return;
            var value = this.$data.hookah[index];
            value.Online = state;
        },

        checkOnline: function () {
            var self = this;
            $.getJSON('/Places/GetOnlineState/'+self.place.Id,
                function(json) {

                    json.online.forEach(function(entry) {
                        self.switchState(entry, true);
                    });

                    json.offline.forEach(function(entry) {
                        self.switchState(entry, false);
                    });
                            
                });
        },

        updateStats: function(hookahCode, stats) {
            var index = this.$data.hookah.map(function(e) { return e.Key; }).indexOf(hookahCode);
            if (index === -1)
                return;
            var value = this.$data.hookah[index];
            value.DynamicSmokeStatistic = stats;
        },
        onLine: function(hookahCode) {
            switchState(hookahCode, true);


        },

        setMetadata: function(hookahCode, estPufCount,picture) {
            var index = this.$data.hookah.map(function(e) { return e.Key; }).indexOf(hookahCode);
            if (index === -1)
                return;
            var value = this.$data.hookah[index];
            value.EstPufCount = estPufCount;
            value.HookahPicture = picture;

        },

        serviceRequest: function(hookahCode) {
            var index = this.$data.hookah.map(function(e) { return e.Key; }).indexOf(hookahCode);
            if (index === -1)
                return;
            var value = this.$data.hookah[index];
            value.Service = true;
            playSound();
        },

        serviceDone: function() {
            this.Service = !this.Service;
        }
    }
});


hub.client.updateStats = v.updateStats;
hub.client.online = v.onLine;
hub.client.setMetadata = v.setMetadata;
hub.client.serviceRequest = v.serviceRequest;

function playSound() {
    var audio = new Audio('/Content/Audio/notification.mp3');
    audio.play();
}

String.prototype.capitalize = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
}