<?php

use Illuminate\Database\Capsule\Manager;
use Slim\App;

require __DIR__ . "/../vendor/autoload.php";

session_start();

$app = new App([
    "settings" => [
        "displayErrorDetails" => true,
        "determineRouteBeforeAppMiddleware" => true,
        "addContentLengthHeader" => false,
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
]);

$container = $app->getContainer();

$capsule = new Manager;
$capsule->addConnection($container["settings"]["db"]);
$capsule->setAsGlobal();
$capsule->bootEloquent();

require __DIR__ . "/../bootstrap/container.php";
require __DIR__ . "/../app/Route/website/routes.php";