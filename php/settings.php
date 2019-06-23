<?php

return [
    "settings" => [
        "displayErrorDetails" => false,
        "determineRouteBeforeAppMiddleware" => true,
        "addContentLengthHeader" => false,
        "trustedProxy" => false,
        "routerCacheFile" => "/../cache/router.cache",
        "view" => [
            "cache" => "/../cache/views"
        ],
        "db" => [
            "driver" => "mysql",
            "host" => "localhost",
            "database" => "secureapp",
            "username" => "justin",
            "password" => "tdXx9EbaLFku2w",
            "charset" => "utf8",
            "collation" => "utf8_unicode_ci",
            "prefix" => ""
        ]
    ]
];