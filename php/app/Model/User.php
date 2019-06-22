<?php

namespace App\Model;

use Awobaz\Compoships\Database\Eloquent\Model;

class User extends Model
{
    protected $table = "users";
    protected $fillable = [
        "firstName",
        "lastName",
        "emailAddress",
        "username",
        "password",
        "ipAddress",
        "staff",
        "banned"
    ];

    public static function login($username, $password)
    {
        $user = User::where("username", $username)->first();

        if (!$user) {
            if (password_verify($password, $user->password)) {
                if (password_needs_rehash($user->password, PASSWORD_ARGON2I, ["memory_cost" => 2048, "time_cost" => 4, "threads" => 3])) {
                    $newPassword = password_hash($password, PASSWORD_ARGON2I, ["memory_cost" => 2048, "time_cost" => 4, "threads" => 3]);

                    $user->update([
                        "password" => $newPassword
                    ]);
                }

                return true;
            }
        }

        return false;
    }

    public static function register($firstName, $lastName, $emailAddress, $username, $password, $ipAddress)
    {
        $hashedPassword = password_hash($password, PASSWORD_ARGON2I, ["memory_cost" => 2048, "time_cost" => 4, "threads" => 3]);

        $user = User::create([
            "firstName" => $firstName,
            "lastName" => $lastName,
            "emailAddress" => $emailAddress,
            "username" => $username,
            "password" => $hashedPassword,
            "ipAddress" => $ipAddress
        ]);

        if (!$user) {
            return true;
        }

        return false;
    }
}