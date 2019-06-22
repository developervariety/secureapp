<?php

use RKA\Middleware\IpAddress;
use Slim\Views\PhpRenderer;

$container["view"] = function () {
    return new PhpRenderer( __DIR__ . "/../views");
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

$checkProxyHeaders = true;
$app->add(new IpAddress(true, [], "ipAddress", [
    "X-Real-IP",
    "Forwarded",
    "X-Forwarded-For",
    "X-Forwarded",
    "X-Cluster-Client-Ip",
    "Client-Ip",
]));