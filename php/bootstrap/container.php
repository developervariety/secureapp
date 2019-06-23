<?php

use Respect\Validation\Validator;
use RKA\Middleware\IpAddress;
use Slim\Views\Twig;
use Slim\Views\TwigExtension;

$container["view"] = function ($container) {
    $view = new Twig(__DIR__ . "/../resources/views", [
        "cache" => false
    ]);
//$container("settings")["view"]["cache"],
    $view->addExtension(new TwigExtension(
        $container->router,
        $container->request->getUri()
    ));

    return $view;
};

$container["validator"] = function () {
    //Respect\Validation\Validator::with("App\\Validator\\Rules");
    return new Validator();
};

$container["notFoundHandler"] = function ($container) {
    return function ($request, $response) use ($container) {
        return $response->withJson([
            "status" => "error",
            "data" => [
                "error" => [
                    "message" => "The file or page you are looking for does not exist.",
                ]
            ],
            "timestamp" => time()
        ])->withStatus(404);
    };
};

$container["notAllowedHandler"] = function ($container) {
    return function ($request, $response, $methods) use ($container) {
        return $response->withJson([
            "status" => "error",
            "data" => [
                "error" => [
                    "message" => "Method is not accepted",
                    "allowed" => implode(", ", $methods)
                ]
            ],
            "timestamp" => time()
        ])->withStatus(405);
    };
};

$container["errorHandler"] = function ($container) {
    return function ($request, $response, $exception) use ($container) {
        return $response->withJson([
            "status" => "error",
            "data" => [
                "message" => "500: Internal Server Error",
                "exception" => [
                    "code" => $exception->getCode(),
                    "message" => $exception->getMessage(),
                    "file" => $exception->getFile(),
                    "line" => $exception->getLine()
                ]
            ],
            "timestamp" => time()
        ])->withStatus(500);
    };
};

$checkProxyHeaders = isset($container->get("settings")["trustedProxy"])? is_array ($container->get("settings")["trustedProxy"])? true : false : false;

$checkProxyHeaders = true;
$app->add(new IpAddress(true, is_array ($container->get("settings")["trustedProxy"])? $container->get("settings")["trustedProxy"] : [], "ipAddress", [
    "X-Real-IP",
    "Forwarded",
    "X-Forwarded-For",
    "X-Forwarded",
    "X-Cluster-Client-Ip",
    "Client-Ip",
]));