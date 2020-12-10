var oauth = (function (angular) {
    return new function () {
        var q = new function () { // query string to object and vice versa
            this.parse = function (queryString) {
                return JSON.parse('{"' + decodeURI(queryString.replace(/&/g, '\",\"').replace(/=/g, '\":\"')) + '"}');
            }
            this.stringify = function (queryObject) {
                var pairs = [];
                for (var prop in queryObject) {
                    pairs.push(encodeURI(prop) + '=' + encodeURI(queryObject[prop]));
                }
                return pairs.join('&');
            }
        };
        challenge = function (target, parameters) {
            //localStorage.removeItem('user');
            var query = { redirect_uri: target.location, client_id: window.oauth.env.client_id, response_type: 'token' };
            query = angular.extend(query, parameters || {});
            var authorizeUrl = window.oauth.env.ssoUrl + '/oauth/authorize?' + q.stringify(query);
            target.location = authorizeUrl;
        }
        signin = function (target) {
            var fragment = /#(.+)$/;
            var match = target.location.href.match(fragment);
            if (match) { // the token is expected
                var auth = q.parse(match[1]);
                angular.extend(auth, window.oauth.env);
                localStorage.setItem('user', JSON.stringify(auth));
                var clean = target.location.href.replace(fragment, '');
                target.history.replaceState({}, '', clean);
                if (!auth.error) {
                    target.setTimeout(function () {
                        window.oauth.refreshToken();
                    }, 1000 * (auth.expires_in * 0.9));
                    //}, 15000);
                    return true;
                } else {
                    console.error('Signin error: ' + auth.error);
                }
            }
        }
        this.refreshToken = function () {
            document.getElementById('oauth-iframe').contentWindow.postMessage('token-refresh', '*');
        }
        this.login = function (ssoUrl, clientId, onBeforeBootstrap) {
            this.env = { client_id: clientId, ssoUrl: ssoUrl };
            if (!signin(window)) {
                challenge(window);
            } else { // bootstrap:
                var ng = { $injector: angular.injector(['ng']) };
                $q = ng.$injector.get('$q');
                $q.when((function () {
                    if (onBeforeBootstrap)
                        return onBeforeBootstrap(ng);
                })(), function () {
                    angular.element(document).ready(function () {
                        angular.bootstrap(document.getElementsByTagName('html')[0], ['app'])
                    });
                });
                // create iframe for background token refresh:
                var iframe = document.createElement('iframe');
                iframe.setAttribute('id', 'oauth-iframe');
                iframe.style.display = 'none';
                iframe.src = 'refresh.html';
                document.body.appendChild(iframe);
                iframe.onload = function () {
                    if (signin(iframe.contentWindow)) {
                        console.info('Token refreshed');
                        if (oauth.onRefresh)
                            oauth.onRefresh();
                    }
                    iframe.contentWindow.onmessage = function (e) {
                        if (e.data == 'token-refresh') {
                            challenge(iframe.contentWindow, { prompt: 'none' });
                        }
                    };
                }
            }
        }
        this.logout = function () {
            var user = angular.fromJson(localStorage.getItem('user'));
            localStorage.removeItem('user');            
            var logoutUri = user.ssoUrl + '/user/signout?' + q.stringify({ redirect_uri: window.location });
            window.location = logoutUri;
        }
        angular.module('auth', [])
            .service('user', ['$window', function ($window) {
                this.current = function () {
                    var user = angular.fromJson(localStorage.getItem('user'));
                    token = user['access_token'];
                    var payload = angular.fromJson($window.atob(token.split('.')[1]));
                    return new function () {
                        this.claims = [];
                        this.claimOf = function (name) {
                            return payload[name];
                        }
                        this.id = this.claimOf('sub');
                        this.env = this.claimOf('edscha:env');
                        this.token = token;
                        this.host = this.claimOf('azp');
                        for (var name in payload) {
                            this.claims.push({ name: name, value: payload[name] });
                        };
                        var roles = this.claimOf('role');
                        if (Array.isArray(roles)) {
                            this.roles = roles;
                        } else {
                            this.roles = [roles];
                        }
                        this.hasRole = function (role) {
                            return this.roles.indexOf(role) >= 0;
                        }
                        this.isFrom = function (env) {
                            return this.env === env;
                        }
                        this.isProductive = this.isFrom('PE3');
                    }
                }
            }])
            .config(['$httpProvider', '$provide', function ($httpProvider, $provide) {
                var timestamp = Date.now();
                $httpProvider.interceptors.push(['$q', 'user', function ($q, user) {
                    return {
                        'request': function (http) {
                            http.headers.Authorization = 'Bearer ' + user.current().token;
                            http.withCredentials = true;
                            var connectionId = localStorage.getItem('signalR');
                            if (connectionId)
                                http.headers.SignalR = 'ConnectionId ' + connectionId;
                            // https://www.reddit.com/r/angularjs/comments/3j77a2/prevent_browser_template_caching/
                            if (http.url.match(/^.*\.tpl\.html$/)) { // prevent caching of templates on ng side
                                http.url = http.url + '?v=' + timestamp;
                                // alternatively, declare an interceptor in the app and inject a version rendered in index.cshtml (at and from server)
                            }
                            return http;
                        },
                        'responseError': function (rejected) {
                            if (rejected.status === 401) { // e.g. cookie is still valid but user credentials have been changed (cookie is not aware of this)
                                oauth.logout();
                            }
                            return $q.reject(rejected);
                        }
                    };
                }]);
                $provide.decorator('$http', ['$delegate', function ($delegate) {
                    //$delegate.parseQuery = function (queryString) {
                    //    var query = JSON.parse('{"' + decodeURI(queryString.replace(/&/g, "\",\"").replace(/=/g, "\":\"")) + '"}');
                    //    return query;
                    //}
                    $delegate.query = q;
                    $delegate.binaryToBase64 = function (binary) {
                        var base64 = btoa([].reduce.call(new Uint8Array(binary), function (buffer, byte) {
                            return buffer + String.fromCharCode(byte)
                        }, ''));
                        return base64;
                    }
                    return $delegate;
                }]);
            }]);
    };
})(angular);