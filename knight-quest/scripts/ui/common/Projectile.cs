using Godot;
using System;
using GodotUtilities;

namespace Game.UI
{
    [Scene]
    public partial class Projectile : Node2D
    {
        [Export] public float Lifetime { get; set; } = 1.5f;
        [Export] public Vector2 Direction { get; set; } = Vector2.Right;
        [Export] public float Speed { get; set; } = 250f;

        private float _timer = 0f;
        private AnimatedSprite2D _anim;

        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated)
                return;

            WireNodes();
        }

        public override void _Ready()
        {
            _anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

            if (_anim != null)
                _anim.Play("shoot");
        }

        public override void _Process(double delta)
        {
            Position += Direction * Speed * (float)delta;
            _timer += (float)delta;
            if (_timer >= Lifetime)
                QueueFree();
        }
    }
}