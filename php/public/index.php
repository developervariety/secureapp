<?php

$settings = include_once __DIR__ . "/../settings.php";

if (PHP_SAPI == "cli-server") {
    $url  = parse_url($_SERVER["REQUEST_URI"]);
    $file = __DIR__ . $url["path"];

    if (is_file($file)) {
        return false;
    }

    $settings["displayErrorDetails"] = true;
    $settings["routerCacheFile"]     = false;
    $settings["view"]["cache"]       = false;

}

require __DIR__ . "/../bootstrap/app.php";

$app->run();