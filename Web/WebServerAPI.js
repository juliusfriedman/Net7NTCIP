/*
JSON RPC WebServer Javascript API Classes
(C) JuliusRFriedman 2011
All Rights Reserved
Requires MooTools 1.3
Created for ASTI Transportation for use with TCM Application, Compatible with anything!
*/
(function (w, d, b, u, nil, $, serverInfo) {
    //Ensure we have a window Object and MooTools is present otherwise return
    if (w == u || $ == u) return;
    if (serverInfo instanceof String) serverInfo = u;
    //Declarations
    var $serverInfo = serverInfo || {
        'Banned': [],
        'Clients': [],
        'Encoding': 'utf-8',
        'DisconnectTime': 24105,
        'Wake': 905,
        'Map': [],
        'InstanceId': 0,
        'Port': 8099,
        'HeaderName': 'login'
    },
    $getUser = function (userName, pulicKey) {
        if (!userName || userName.isNullOrEmpty()) return nil;
        var results = $serverInfo.Clients.filter(function (user) { return user.toString() === userName });
        if (pulicKey) results = results.filter(function (user) { return user.getPublicKey() === publicKey; })[0];
        else return results[0];
    },
    $currentUser = nil,
    $updateLiteral = function (literal) {
        try {
            this.$literal = literal;
            this.$literal.hide();
            this.toElement().redraw.attempt(nil, this);
        } catch (Error) { return; }
    },
    $listenRequest = new Request.JSON({
        url: 'ServerPush.ashx/Listen',
        link: 'ignore',
        method: 'post',
        async: true,
        timeout: $serverInfo.DisconnectTime + $serverInfo.Wake,
        urlEncoded: true,
        encoding: $serverInfo.Encoding,
        noCache: true,
        evalScripts: true,
        evalResponse: true,
        user: nil,
        password: nil,
        headers: {
            'login': (Cookie.read($serverInfo.HeaderName || 'login') || nil)
        },
        events: {
            'onRequest': function () {
                try {
                    console.log('ListenRequest onRequest');
                    WebServerClient.listening = true;
                } catch (Error) { return; }
            },
            'onLoadstart': function (event, xhr) {
                try {
                    console.log('ListenRequest onLoadstart');
                } catch (Error) { return; }
            },
            'onProgress': function (event, xhr) {
                try {
                    var loaded = event.loaded, total = event.total;
                    console.log('ListenRequest onProgress ' + parseInt(loaded / total * 100, 10));
                } catch (Error) { return; }
            },
            'onComplete': function () {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onComplete');
                } catch (Error) { return; }
            },
            'onCancel': function () {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onCancel');
                } catch (Error) { return; }
            },
            'onSuccess': function (responseText, responseXML) {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onSuccess');
                } catch (Error) { return; }
            },
            'onFailure': function (xhr) {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onFailure');
                } catch (Error) { return; }
            },
            'onException': function (headerName, value) {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onException');
                } catch (Error) { return; }
            },
            'onTimeout': function () {
                try {
                    WebServerClient.listening = false;
                    console.log('ListenRequest onTimeout');
                } catch (Error) { return; }
            }
        }
    }),
    $sendRequest = new Request.JSON({
        url: 'ServerPush.ashx/Send',
        link: 'chain',
        method: 'post',
        async: true,
        timeout: 5000 + $serverInfo.Wake,
        urlEncoded: true,
        encoding: $serverInfo.Encoding,
        noCache: true,
        evalScripts: true,
        evalResponse: true,
        user: nil,
        password: nil,
        headers: {
            'login': (Cookie.read($serverInfo.HeaderName || 'login') || nil)
        },
        events: {
            'onRequest': function () {
                try {
                    console.log('SendRequest onRequest');
                    WebServerClient.transmitting = true;
                } catch (Error) { return; }
            },
            'onLoadstart': function (event, xhr) {
                try {
                    console.log('SendRequest onLoadStart');
                } catch (Error) { return; }
            },
            'onProgress': function (event, xhr) {
                try {
                    var loaded = event.loaded, total = event.total;
                    console.log('SendRequest onProgress ' + parseInt(loaded / total * 100, 10));
                } catch (Error) { return; }
            },
            'onComplete': function () {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onComplete');
                } catch (Error) { return; }
            },
            'onCancel': function () {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onCancel');
                } catch (Error) { return; }
            },
            'onSuccess': function (responseText, responseXML) {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onSuccess');
                } catch (Error) { return; }
            },
            'onFailure': function (xhr) {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onFailure');
                } catch (Error) { return; }
            },
            'onException': function (headerName, value) {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onException');
                } catch (Error) { return; }
            },
            'onTimeout': function () {
                try {
                    WebServerClient.transmitting = false;
                    console.log('SendRequest onTimeout');
                } catch (Error) { return; }
            }
        }
    }),
    //Ensure WebServer message Commands match these hash keys. If they do not they message will be dropped silently.
    $ordinals = {
        'message': function (message) {
            try {
                //This is the general handler update the main interface
            } catch (Error) { return; }
        },
        'chatMessage': function (message) {
            try {
                var chatDirectory = WebServerClient.chats || {},
                chatInfo = message.Payload;
                var webChat = chatDirectory[chatInfo.RoomId];
                webChat.update(chatInfo.User, chatInfo.Message);
            } catch (Error) { return; }
        },
        'joinChat': function (message) {
            try {
                var userInfo = message.Payload.User,
                chatInfo = message.Payload.ChatInfo;
                chatInfo.User = userInfo;
                $ordinals['chatMessage']({ User: userInfo, RoomId: chatInfo.RoomId, Message: userInfo.toString() + ' has entered the chat!' });
                $ordinals['chatInfo']({ Payload: chatInfo });
            } catch (Error) { return; }
        },
        'leaveChat': function (message) {
            try {
                var userInfo = message.Payload.User,
                chatInfo = message.Payload.ChatInfo;
                chatInfo.User = userInfo;
                //Get the chat from the chatDirectory
                //Call close on the webChat
            } catch (Error) { return; }
        },
        'sendTo': function (message) {
            //Determine who the message is from
            //If there is a conversation already available update it.
            //Else display a confirmation to start the conversation
            try {
                var conversationDirectory = WebServerClient.conversations || {},
                    conversationInfo = message.Payload;
                var webConversation = conversationDirectory[conversationInfo.PublicKey] || new WebConversation(conversationInfo.PublicKey);
                WebServerClient.conversations = conversationDirectory;
                WebServerClient.conversations[conversationInfo.PublicKey] = webConversation;
                webConversation.update(conversationInfo.Message);
            } catch (Error) { return; }
        },
        'userList': function (message) {
            $serverInfo.Clients = [];
            try {
                Array.from(message.Payload).forEach(function (user) {
                    user = new WebServerUser(user);
                    $serverInfo.Clients.include(user);
                });
            }
            catch (Error) { return; }
        },
        'chatInfo': function (message) {
            try {
                var chatDirectory = WebServerClient.chats || {},
                chatInfo = message.Payload;
                var webChat = chatDirectory[chatInfo.RoomId] || new WebChat(chatInfo);
                WebServerClient.chats = chatDirectory;
                WebServerClient.chats[chatInfo.RoomId] = webChat;
                webChat.refresh(chatInfo);
            } catch (Error) { return; }
        },
        'shareInfo': function (message) {
            try {
                var shareDirectory = WebServerClient.shares || {},
                shareInfo = message.Payload;
                var webShare = shareDirectory[shareInfo.ShareId] || new WebShare(shareInfo);
                WebServerClient.shares = shareDirectory;
                WebServerClient.shares[shareInfo.ShareId] = webShare;
            } catch (Error) { return; }
        },
        'webServerInfo': function (message) {
            var _serverInfo = $serverInfo;
            try {
                $serverInfo.Banned = message.Payload.Banned || $serverInfo.Banned;
                $serverInfo.Clients = message.Payload.Clients || $serverInfo.Clients;
                $serverInfo.DisconnectTime = message.Payload.DisconnectTime || $serverInfo.DisconnectTime;
                $serverInfo.Wake = message.Payload.Wake || $serverInfo.Wake;
                $serverInfo.Encoding = message.Payload.Encoding || 'utf-8';
                $serverInfo.Map = message.Payload.Map || $serverInfo.Map;
                $serverInfo.Port = message.Payload.Port || $serverInfo.Port;
                $serverInfo.HeaderName = message.Payload.HeaderName || $serverInfo.HeaderName;
                $serverInfo.InstanceId = message.Payload.InstanceId || $serverInfo.InstanceId;
            }
            catch (Error) { $serverInfo = _serverInfo; }
        },
        'welcomeClient': function (message) {
            try {
                $currentUser = new WebServerUser(JSON.decode(message.Payload));
                //WebServerClient.connected = true;
            }
            catch (Error) { WebServerClient.connected = false; return; }
        },
        'goodbyeClient': function (message) {
            try {
                //WebServerClient.connected = false;
                $currentUser = nil;
            } catch (Error) { return; }
        },
        'terminate': function (message) {
        },
        'invite': function (message) {
        },
        'invalidate': function (message) {
            return message && message.Timestamp == u && message.Command == u && $ordinals[message.Command] == u;
        }
    },
    $responseHandler = function (message) {
        try {
            if (!message || $ordinals.invalidate(message)) return;
            $ordinals[message.Command](message);
        }
        catch (Error) { }
    },
    /*
    Classes
    ---------------------
    WebServerClient class
    */
    WebServerClient = new Class({
        Implements: [Options, Events],
        Singleton: true,
        options: {
            retryTimeout: 333,
            connect: false
        },
        events: {
            'connect': function (e) {
            },
            'disconnect': function (e) {
            },
            'message': function (e) {
            }
        },
        connected: false,
        listening: false,
        transmitting: false,
        initialize: function (options) {
            this.setOptions(options);
            if (this.options.connect) this.connect();
        },
        /*
        WebServerMessage - {
        Timestamp: Number,
        Command: String,
        Payload: Object,
        AcknowledgementRequired: Boolean,
        MessageId: String
        }
        */
        _handleResponse: function (response) {
            if (!response || response.xhr === u) return;
            WebServerClient.fireEvent('message', arguments);
            $responseHandler.attempt(response.xhr);
        } .protect(),
        _listen: function (force) {
            if (this.listening && !force) return;
            else if ($listenRequest.isRunning()) $listenRequest.cancel();
            $listenRequest.addEvent('onComplete:once', function () {
                this._listen.pass(this.listening = false).delay(0);
                WebServerClient._handleResponse.pass($sendRequest).delay(0 + 1);
            });
            $listenRequest.send("Timestamp=" + Date.now());
            this.listening = true;
        } .protect(),
        _send: function (command, payload, callBack, requireAck) {
            if (!this.connected || !command) return;
            $sendRequest.addEvent('onComplete:once', function () {
                WebServerClient.transmitting = false;
                WebServerClient._handleResponse.pass($sendRequest).delay(0);
                callBack.attempt(arguments);
            });
            $sendRequest.send({ Timestamp: Date.now(), Command: command, Payload: payload, AcknowledgementRequired: requireAck || false });
        } .protect(),
        sendMessage: function (command, dataz, callBack, requireAck) {
            callBack = Function.from(callBack);
            if (!command || !this.connected) return;
            if (dataz instanceof Array) Array.forEach(dataz, function (item) {
                this.sendMessage(command, item, callBack, requireAck);
            }, this);
            try {
                if (!dataz instanceof String) dataz = JSON.encode(dataz);
                this._send(command, dataz, callBack, requireAck);
            }
            catch (Error) { return; }
        },
        connect: function (callBack) {
            callBack = Function.from(callBack);
            if (this.connected && !this.transmitting) return callBack();
            this._send("connect", nil, function (e) {
                WebServerClient.connected = e.isSuccess();
                WebServerClient.fireEvent('connect', arguments);
                callBack.attempt(arguments);
            });
        },
        disconnect: function (callBack) {
            callBack = Function.from(callBack);
            if (!this.connected && !this.transmitting) return callBack();
            this._send("disconnect", nil, function () {
                WebServerClient.connected = false;
                callBack.attempt(arguments);
                if ($listenRequest.isRunning()) $listenRequest.cancel();
                $listenRequest.removeEvents();
                WebServerClient.removeEvents();
                $sendRequest.removeEvents.delay(0);
                WebServerClient.fireEvent('disconnect', arguments);
                CollectGarbage.attempt();
            });
        },
        refresh: function () {
            this.sendMessage('getInfo');
        },
        getServerInfo: function () {
            return Object.clone($serverInfo);
        },
        getClients: function () {
            return $serverInfo.Clients;
        },
        getBanned: function () {
            return $serverInfo.Banned;
        },
        getInstanceId: function () {
            return $serverInfo.InstanceId;
        },
        getMap: function () {
            return $serverInfo.Map;
        },
        getEncoding: function () {
            return $serverInfo.Encoding;
        },
        getDisconnectTime: function () {
            return $serverInfo.DisconnectTime;
        },
        getWake: function () {
            return $serverInfo.Wake;
        },
        getPort: function () {
            return $serverInfo.Port;
        }
    }),
    /*
    WebServerUser - {
    'ListenId':Number,
    'LastListen':Number,
    'UserData':String,
    'PublicKey':String
    }
    */
    WebServerUser = new Class({
        Implements: [Options, Events],
        TrackInstances: true,
        initialize: function (literal) {
            this._update(literal);
        },
        getUserData: function () {
            return this.$literal.UserData || nil;
        },
        getLastListen: function () {
            return this.$literal.LastListenTime || nil;
        },
        getListenId: function () {
            return this.$literal.ListenId || nil;
        },
        getPublicKey: function () {
            return this.$literal.PublicKey || nil;
        },
        send: function (message, callBack) {
            try {
                if (!message instanceof String) message = JSON.encode(message);
                WebServerClient.sendMessage('sendTo', {
                    'PublicKey': this.getPublicKey(),
                    'From': ($currentUser.getPublicKey() || Cookie('login')),
                    'Message': message
                }, callBack);
            }
            catch (Error) { return; }
        },
        toString: function () {
            try {
                return JSON.decode(this.getUserData()).Username || this.getPublicKey();
            } catch (Error) { return '[WebServerUser Unknown]'; }
        },
        refresh: WebServerClient.refresh,
        _update: $updateLiteral.bind(this).protect()
    }),
    //A private conversation between two users
    WebConversation = new Class({
        Implements: [Options, Events],
        TrackInstances: true,
        remoteUser: nil,
        initialize: function (remoteUser) {
            this.remoteUser = remoteUser;
        } .overloadSetter(),
        sendMessage: function (what) {
            remoteUser.send($currentUser, what, nil);
        },
        update: function (message) {
            try {
                //Get the element representation of the chat
                var element = this.toElement();
                //Get the messagePanel
                var messagPanel = element.messagePanel,
                //Determinw which user the message was from
                webUser = message.From === $currentUser.PublicKey ? $currentUser : remoteUser;
                //Update the web conversation
                messagPanel.grab(new Element(messagePanel.get('elementType') || 'div', messagePanel.get('elementOptions') || {
                    html: webUser.toString() + ' : <b>' + message + '</b>',
                    className: 'pmLine',
                    styles: {
                        height: 'auto',
                        width: 'auto'
                    },
                    events: {
                        click: function (e) {
                            //this.highlight.attempt(nil, this);
                        }
                    }
                }));
            } catch (Error) { return; }
        }
    }),
    //A WebChat which is owned by a WebServerUser
    WebChat = new Class({
        Implements: [Options, Events],
        TrackInstances: true,
        initialize: function (literal) {
            this._update(literal);
        },
        userEnter: function (webUser) {
            try {
                //Door Open Sound
                this.update(webUser, 'Has joined the chat');
            } catch (Error) { return; }
        },
        userLeave: function (webUser) {
            try {
                //Door Close Sound
                this.update(webUser, 'Has left the chat');
            } catch (Error) { return; }
        },
        update: function (webUser, message) {
            try {
                //Get the element representation of the chat
                var element = this.toElement();
                //Get the messagePanel
                var messagPanel = element.messagePanel;
                //Update chat messages
                messagPanel.grab(new Element(messagePanel.get('elementType') || 'div', messagePanel.get('elementOptions') || {
                    html: webUser.toString() + ' : <b>' + message + '</b>',
                    className: 'chtLine',
                    styles: {
                        height: 'auto',
                        width: 'auto'
                    },
                    events: {
                        click: function (e) {
                            //this.highlight.attempt(nil, this);
                        }
                    }
                }));
            } catch (Error) { return; }
        },
        refresh: function (chatInfo) {
            if (chatInfo) this._update.attempt(chatInfo, nil);
            else WebServerClient.sendMessage('refreshChat', this.$literal.RoomID, this._update);
        },
        _update: $updateLiteral.bind(this).protect()
    }),
    //A WebShare which is owned by a WebServerUser
     WebShare = new Class({
         Implements: [Options, Events],
         TrackInstances: true,
         initialize: function (literal) {
             this._update(literal);
         },
         _update: $updateLiteral.bind(this).protect(),
         getOwner: function () {
             return this.$literal.Owner || nil;
         }
     }),
    //An invitation to do something from someone
     WebInvite = new Class({
         Implements: [Options, Events],
         initialize: function (literal) {
             return;
         },
         accept: function () {
         },
         deny: function () {
         }
     });

    //Alias and Export
    w.WebServerClient = new WebServerClient();
    w.WebServerUser = WebServerClient.WebServerUser = WebServerUser;
    w.WebConversation = WebServerClient.WebConversation = WebConversation;
    w.WebChat = WebServerClient.WebChat = WebChat;
    w.WebShare = WebServerClient.WebShare = WebShare;
    w.WebInvite = WebServerClient.WebInvite = WebInvite;

})(window, document, document.body, undefined, null, document.id, '<ServerObject>');