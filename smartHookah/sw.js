'use strict';
var CACHE_NAME = 'psw-cache-v1';
var urlsToCache = [
 
];
self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME).then(function (cache) {
            return cache.addAll(urlsToCache);
        })
    );
});

    self.addEventListener('fetch', function (event) {
        // Intercept fetch request
        event.respondWith(
            // match and serve cached asset if it exists
            caches.match(event.request).then(function (response) {
                return response || fetch(event.request);
            })
        );
});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        // Open our apps cache and delete any old cache items
        caches.open(CACHE_NAME).then(function (cacheNames) {
            cacheNames.keys().then(function (cache) {
                cache.forEach(function (element, index, array) {
                    if (urlsToCache.indexOf(element) === -1) {
                        caches.delete(element);
                    }
                });
            });
        })
    );
});