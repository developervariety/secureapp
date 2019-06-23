<?php

use RKA\Middleware\IpAddress;

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