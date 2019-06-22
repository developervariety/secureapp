<?php

namespace App\Model\Keys;

use Awobaz\Compoships\Database\Eloquent\Model;

class Announcement extends Model
{
    protected $table = "announcements";
    protected $fillable = [
        "application",
        "title",
        "body"
    ];
}