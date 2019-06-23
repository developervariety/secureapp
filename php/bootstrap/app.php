<?php

require __DIR__ . '/../vendor/autoload.php';

use Illuminate\Database\Capsule\Manager;
use Respect\Validation\Validator as v;
use Slim\App;

$app = new App([
    $settings
]);

$container = $app->getContainer();

$capsule = new Manager;
$capsule->addConnection($settings["settings"]["db"]);
$capsule->setAsGlobal();
$capsule->bootEloquent();

require __DIR__ . "/../bootstrap/container.php";
require __DIR__ . "/../bootstrap/controllers.php";
require __DIR__ . "/../bootstrap/middleware.php";

v::with("App\\Validation\\Rules");

require __DIR__ . "/../app/Route/Website/routes.php";
require __DIR__ . "/../app/Route/Api/routes.php";