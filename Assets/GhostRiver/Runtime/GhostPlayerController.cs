using UnityEngine;
using UnityEngine.EventSystems;

namespace GhostRiver
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class GhostPlayerController : MonoBehaviour
    {
        private const float MoveSpeed = 1.65f;
        private const float JumpSpeed = 3.65f;
        private const float DashSpeed = 4.65f;
        private const float DashDuration = 0.27f;
        private const float MaxStamina = 100f;
        private const float StaminaRecovery = 38f;

        private GhostRiverGame game;
        private Rigidbody2D body;
        private BoxCollider2D hitbox;
        private SpriteRenderer sprite;
        private Sprite idleSprite;
        private Sprite walkSpriteA;
        private Sprite walkSpriteB;
        private Sprite jumpSprite;
        private Sprite dashSprite;
        private float facing = 1f;
        private float dashTimer;
        private float jumpBuffer;
        private float coyoteTimer;
        private float attackCooldown;
        private float comboWindow;
        private float staminaDelay;
        private float invulnerability;
        private float stressDelay;
        private float debuffTimer;
        private float stamina = MaxStamina;
        private float stress;
        private int comboStep;
        private int health = 10;
        private int potions = 3;
        private bool attackQueued;
        private bool touchLeft;
        private bool touchRight;
        private float checkpointX = 0.68f;

        public int CurrentHp => health;
        public int MaxHp => 10;
        public int Potions => potions;
        public float CheckpointX => checkpointX;
        public float CurrentStamina => stamina;
        public float Stress => stress;
        public float DebuffTimer => debuffTimer;
        public int ComboStep => comboStep;
        public bool IsDashing => dashTimer > 0f;
        public float Facing => facing;
        public bool IsInvulnerable => invulnerability > 0f || IsDashing;
        public Vector2 Velocity => body != null ? body.linearVelocity : Vector2.zero;

        public void Initialize(GhostRiverGame owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<BoxCollider2D>();
            sprite = GetComponent<SpriteRenderer>();
            idleSprite = LoadPixelSprite("orb");
            walkSpriteA = LoadPixelSprite("orb_walk_a");
            walkSpriteB = LoadPixelSprite("orb_walk_b");
            jumpSprite = LoadPixelSprite("orb_jump");
            dashSprite = LoadPixelSprite("orb_dash");

            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            hitbox.size = new Vector2(0.17f, 0.2f);
            checkpointX = 0.68f;
            ResetState();
        }

        private void Update()
        {
            if (game == null || !game.GameStarted || game.IsPaused || game.HasWon || game.IsCatalogOpen)
            {
                if (body != null && game != null && !game.IsPaused)
                    body.linearVelocity = Vector2.zero;
                return;
            }

            var dt = Time.deltaTime;
            var grounded = IsGrounded();
            if (grounded)
                coyoteTimer = 0.12f;
            else
                coyoteTimer = Mathf.Max(0f, coyoteTimer - dt);

            if (jumpBuffer > 0f)
                jumpBuffer = Mathf.Max(0f, jumpBuffer - dt);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                jumpBuffer = 0.14f;

            if (jumpBuffer > 0f && (grounded || coyoteTimer > 0f) && dashTimer <= 0f)
            {
                var jumpScale = debuffTimer > 0f ? 0.78f : 1f;
                body.linearVelocity = new Vector2(body.linearVelocity.x, JumpSpeed * jumpScale);
                jumpBuffer = 0f;
                coyoteTimer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                StartDash();
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(0))
                attackQueued = true;
            if (Input.GetKeyDown(KeyCode.H))
                UsePotion();

            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            comboWindow = Mathf.Max(0f, comboWindow - dt);
            if (comboWindow <= 0f)
                comboStep = 0;
            staminaDelay = Mathf.Max(0f, staminaDelay - dt);
            if (staminaDelay <= 0f)
                stamina = Mathf.Min(MaxStamina, stamina + StaminaRecovery * dt);
            stressDelay = Mathf.Max(0f, stressDelay - dt);
            if (stressDelay <= 0f)
                stress = Mathf.Max(0f, stress - 14f * dt);
            debuffTimer = Mathf.Max(0f, debuffTimer - dt);
            invulnerability = Mathf.Max(0f, invulnerability - dt);

            var axis = GetMovementAxis();
            if (Mathf.Abs(axis) > 0.01f)
            {
                facing = Mathf.Sign(axis);
                sprite.flipX = facing < 0f;
            }

            if (dashTimer > 0f)
                dashTimer = Mathf.Max(0f, dashTimer - dt);

            var pose = dashTimer > 0f ? dashSprite : !grounded ? jumpSprite :
                Mathf.Abs(axis) > 0.01f
                    ? (Mathf.FloorToInt(Time.time * 8f) % 2 == 0 ? walkSpriteA : walkSpriteB)
                    : idleSprite;
            if (pose != null && sprite.sprite != pose)
                sprite.sprite = pose;

            if (transform.position.y < -0.5f)
                Respawn("Rơi khỏi đường đi! Quay lại điểm kiểm soát gần nhất.");

            game.RefreshHud();
        }

        private void FixedUpdate()
        {
            if (game == null || !game.GameStarted || game.IsPaused || game.HasWon || game.IsCatalogOpen)
                return;

            if (attackQueued)
            {
                attackQueued = false;
                TryAttack();
            }

            if (dashTimer > 0f)
            {
                body.linearVelocity = new Vector2(facing * DashSpeed, 0f);
                return;
            }

            var axis = GetMovementAxis();
            var movementScale = debuffTimer > 0f ? 0.62f : 1f;
            body.linearVelocity = new Vector2(axis * MoveSpeed * movementScale, body.linearVelocity.y);
        }

        public void ResetState()
        {
            if (body == null)
                return;

            health = MaxHp;
            potions = 3;
            stamina = MaxStamina;
            stress = 0f;
            stressDelay = 0f;
            debuffTimer = 0f;
            dashTimer = 0f;
            jumpBuffer = 0f;
            coyoteTimer = 0f;
            attackCooldown = 0f;
            comboWindow = 0f;
            comboStep = 0;
            staminaDelay = 0f;
            invulnerability = 0f;
            checkpointX = 0.68f;
            facing = 1f;
            attackQueued = false;
            touchLeft = false;
            touchRight = false;
            body.linearVelocity = Vector2.zero;
            body.position = game.GetPlayerSpawnPosition();
            sprite.flipX = false;
        }

        public void SetCheckpoint(float worldX)
        {
            checkpointX = worldX;
        }

        public void Respawn(string message)
        {
            if (body == null)
                return;

            dashTimer = 0f;
            attackQueued = false;
            jumpBuffer = 0f;
            body.linearVelocity = Vector2.zero;
            body.position = game.GetSpawnPosition(checkpointX);
            invulnerability = 1.4f;
            game.Say(message, 2.2f);
        }

        public void TakeDamage(Vector2 sourcePosition)
        {
            if (IsInvulnerable || game == null)
                return;

            health = Mathf.Max(0, health - 1);
            invulnerability = 0.9f;
            comboStep = 0;
            comboWindow = 0f;
            stress = Mathf.Min(100f, stress + 30f);
            stressDelay = 4f;
            body.linearVelocity = new Vector2(transform.position.x < sourcePosition.x ? -1.25f : 1.25f, 1.45f);

            var overloaded = stress >= 100f;
            if (overloaded)
            {
                stress = 0f;
                debuffTimer = 5f;
                game.Say("Quá tải! Tốc độ giảm trong 5 giây.", 2.4f);
            }
            else if (health > 0)
            {
                game.Say("Bạn bị trúng đòn! Còn " + health + " máu.", 1.5f);
            }

            if (health <= 0)
            {
                health = MaxHp;
                stress = 0f;
                stressDelay = 0f;
                debuffTimer = 0f;
                Respawn("Bạn đã gục. Hãy thử đánh hoặc né đòn!");
            }
            game.RefreshHud();
        }

        public void UsePotion()
        {
            if (!game.GameStarted || game.HasWon || game.IsCatalogOpen || potions <= 0 || health >= MaxHp)
                return;

            potions--;
            var healed = Mathf.Min(4, MaxHp - health);
            health += healed;
            game.Say("Hồi +" + healed + " máu.", 1.3f);
            game.RefreshHud();
        }

        public void SetTouchDirection(GhostMobileAction action, bool pressed)
        {
            if (action == GhostMobileAction.Left)
                touchLeft = pressed;
            else if (action == GhostMobileAction.Right)
                touchRight = pressed;
        }

        public void TriggerTouchAction(GhostMobileAction action)
        {
            if (action == GhostMobileAction.Jump)
                jumpBuffer = 0.14f;
            else if (action == GhostMobileAction.Dash)
                StartDash();
            else if (action == GhostMobileAction.Attack)
                attackQueued = true;
            else if (action == GhostMobileAction.Potion)
                UsePotion();
        }

        private float GetMovementAxis()
        {
            var left = touchLeft || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
            var right = touchRight || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
            return (right ? 1f : 0f) - (left ? 1f : 0f);
        }

        private static Sprite LoadPixelSprite(string spriteName)
        {
            var sprite = Resources.Load<Sprite>("GhostRiverArt/" + spriteName);
            if (sprite == null)
                Debug.LogError("Missing pixel-art sprite: Resources/GhostRiverArt/" + spriteName + ".png");
            return sprite;
        }

        private void StartDash()
        {
            if (dashTimer > 0f)
                return;
            dashTimer = DashDuration;
            coyoteTimer = 0f;
            body.linearVelocity = new Vector2(facing * DashSpeed, 0f);
        }

        private void TryAttack()
        {
            if (attackCooldown > 0f)
                return;

            var nextStep = comboWindow > 0f ? comboStep % 3 + 1 : 1;
            var cost = nextStep == 1 ? 18f : nextStep == 2 ? 22f : 32f;
            if (stamina < cost)
            {
                comboStep = 0;
                comboWindow = 0f;
                game.Say("Hụt hơi! Chờ thể lực hồi rồi đánh tiếp.", 0.9f);
                return;
            }

            comboStep = nextStep;
            comboWindow = 0.78f;
            stamina -= cost;
            staminaDelay = 0.72f;
            attackCooldown = 0.24f;
            game.ShowAttack(this, nextStep);
            game.ApplyAttack(this, nextStep);
        }

        private bool IsGrounded()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = GhostRiverGame.GroundMask,
                useTriggers = false
            };
            var hits = new RaycastHit2D[2];
            return Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size * 0.9f, 0f,
                Vector2.down, filter, hits, 0.025f) > 0;
        }

    }

    public enum GhostMobileAction
        {
            Left,
            Right,
            Jump,
            Dash,
            Attack,
            Potion
        }

    public sealed class GhostMobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
        {
            private GhostPlayerController player;
            private GhostMobileAction action;
            private bool held;

            public void Initialize(GhostPlayerController owner, GhostMobileAction buttonAction)
            {
                player = owner;
                action = buttonAction;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (action == GhostMobileAction.Left || action == GhostMobileAction.Right)
                {
                    held = true;
                    player.SetTouchDirection(action, true);
                }
                else
                {
                    player.TriggerTouchAction(action);
                }
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                Release();
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                Release();
            }

            private void Release()
            {
                if (!held)
                    return;
                held = false;
                player.SetTouchDirection(action, false);
            }
        }
}
