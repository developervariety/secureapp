<?php

use App\Model\User;
use Respect\Validation\Validator as V;
use Slim\Http\Request;
use Slim\Http\Response;

$app->group("/auth", function () use ($app) {
    $this->post("/login", function (Request $request, Response $response) {
        $validator = $this->validator->validate($request, [
            "username" => V::length(6, 45)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Username"),
            "password" => V::length(6, 255)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Password")
        ]);

        if ($validator->isValid()) {
            $body = $request->getParsedBody();

            if (User::login($body["username"], $body["password"])) {
                return $response->withJson([
                    "status" => "success",
                    "data" => [
                        "message" => "You've successfully logged in."
                    ]
                ]);
            } else {
                return $response->withJson([
                    "status" => "error",
                    "data" => [
                        "message" => "You've entered an incorrect username or password."
                    ]
                ]);
            }
        } else {
            $errors = $validator->getErrors();

            return $response->withJson([
                "status" => "error",
                "data" => $errors
            ]);
        }
    });

    $this->post("/register", function (Request $request, Response $response) {
        $validator = $this->validator->validate($request, [
            "firstName" => V::length(2, 25)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Username"),
            "lastName" => V::length(2, 255)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Username"),
            "emailAddress" => V::length(6, 255)
                ->email()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Email Address"),
            "username" => V::length(6, 45)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Username"),
            "password" => V::length(6, 255)
                ->alnum()
                ->noWhitespace()
                ->notEmpty()
                ->setName("Password"),
            "confirmPassword" => V::equals($request->getParam("password"))
                ->setName("Password Confirmation")
        ]);

        if ($validator->isValid()) {
            $body = $request->getParsedBody();

            if (User::register($body["firstName"], $body["lastName"], $body["emailAddress"], $body["username"], $body["password"], $request->getAttribute("ipAddress"))) {
                return $response->withJson([
                    "status" => "success",
                    "data" => [
                        "message" => "You've successfully registered, you may now log in."
                    ]
                ]);
            } else {
                return $response->withJson([
                    "status" => "error",
                    "data" => [
                        "message" => "An unexpected error occurred, please try again later."
                    ]
                ]);
            }
        } else {
            $errors = $validator->getErrors();

            return $response->withJson([
                "status" => "error",
                "data" => $errors
            ]);
        }
    });
});