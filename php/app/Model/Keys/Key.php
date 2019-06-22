<?php

namespace App\Model\Keys;

use Awobaz\Compoships\Database\Eloquent\Model;

class Key extends Model
{
    protected $table = "keys";
    protected $fillable = [
        "application",
        "value",
        "type",
        "rule",
        "tracking",
        "expiration",
        "activated",
        "banned"
    ];
}