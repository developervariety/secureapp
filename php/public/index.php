<?php

$settings = include_once __DIR__ . "/../settings.php";
$debug = file_exists(__DIR__ . "/../.debug") ? true: false;

if (PHP_SAPI == "cli-server") {
    $url  = parse_url($_SERVER["REQUEST_URI"]);
    $file = __DIR__ . $url["path"];

    if (is_file($file)) {
        return false;
    }

    $debug = true;
}

if (!$debug) {
    $settings["displayErrorDetails"] = true;
    $settings["routerCacheFile"] = false;
    $settings["view"]["cache"] = false;
}

require __DIR__ . "/../bootstrap/app.php";

$app->run();