using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SlideGame
{
	public enum Direction { Down, Left, Right, Up }
	public class Child : Sprite
	{
		#region Fields
		private int speed;
		private bool isMoving;
		private int currentCheckPoint;
		private Vector2 movingOffset;
		private Direction currentDirection;
		#endregion
		
		#region Properties
		#region Public
		public int Age { get; set; }
		public bool IsPlaying { get; set; }
		public Vector2 NextPosition { get; set; }
		#endregion

		#region Private
		// Độ dời tọa độ sau mỗi lần di chuyển.
		private Vector2 MovingOffset
		{
			get
			{
				movingOffset.X = (float)
					(
					speed * Math.Cos(GetAngle(this.position, NextPosition))
					);

				movingOffset.Y = (float)
					(
					speed * Math.Sin(GetAngle(this.position, NextPosition))
					);
				return movingOffset;
			}
		}
		#endregion
		#endregion

		#region Constructors
		/// <summary>
		/// Nếu ko truyền vào tham số millisecondsPerFrame thì sử dụng 
		/// defaultMillisecondsPerFrame = 16 (mặc định)
		/// </summary>
		public Child
			(
			Texture2D textureImage, Vector2 position, float rotationAngle,
			Point frameSize, Point sheetSize, float drawLayer, int age, int speed
			)
			: this
			(
			textureImage, position, rotationAngle, frameSize, sheetSize, 
			drawLayer, age, speed, defaultMillisecondsPerFrame
			)
		{
			// Bỏ trống vì đã gọi Constructor bên dưới.
		}

		/// <summary>
		/// Ngược lại, sử dụng millisecondsPerFrame từ Constructor.
		/// </summary>
		public Child
			(
			Texture2D textureImage, Vector2 position, float rotationAngle,
			Point frameSize, Point sheetSize, float drawLayer, int age, int speed,
			int millisecondsPerFrame
			)
			: base
			(textureImage, position, rotationAngle, frameSize, sheetSize, 
			drawLayer, millisecondsPerFrame)
			
		{
			this.Age = age;
			this.IsPlaying = false;
			this.NextPosition = Vector2.Zero;
			this.speed = speed;
			this.isMoving = false;
			this.currentDirection = Direction.Left;
			this.currentFrame.Y = (int)currentDirection;
		}
		#endregion

		#region Main_Methods
		public override void Update(GameTime gameTime)
		{
			// Di chuyển đến địa điểm NextPosition.
			MoveTo(NextPosition);

			// Nếu bé đang chơi thì thực hiện các thao tác sau:
			if (this.IsPlaying)
			{
				// Xác định hướng xoay mặt và góc quay,... tùy theo vị trí
				// hiện tại.
				switch(currentCheckPoint)
				{
					case 0:
						this.currentDirection = Direction.Up;
						break;
					case 1:
						this.currentDirection = Direction.Right;
						this.RotationAngle = (float)(Math.PI / 36.0);
						break;
					case 2:
						this.currentDirection = Direction.Right;
						this.RotationAngle = -(float)(Math.PI / 4.0);
						this.isMoving = false;
						break;
					case 3:
						this.currentDirection = Direction.Right;
						this.RotationAngle = 0;
						break;
					case 4:
						this.currentDirection = Direction.Down;
						break;
					default:
						this.IsPlaying = false;
						SlideGame.PlayersList.Remove(this);
						SlideGame.ChildrenQueue.Enqueue(this);
						return;
				}

				// Đọc NextPosition từ bảng Checkpoints.
				NextPosition = SlideGame.Checkpoints[currentCheckPoint];
				if (position == SlideGame.Checkpoints[currentCheckPoint])
					++currentCheckPoint;
			}

			// Ngược lại, bé đang đứng trong hàng đợi -> Xoay mặt sang trái.
			else
			{
				this.currentDirection = Direction.Left;
				this.currentFrame.Y = (int)currentDirection;
			}

			// Nếu bé đang di chuyển thì update Animation.
			if (this.isMoving)
			{
				currentFrame.Y = (int)currentDirection;
				timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
				if (timeSinceLastFrame > millisecondsPerFrame)
				{
					timeSinceLastFrame = 0;
					++currentFrame.X;
					if (currentFrame.X >= sheetSize.X)
					{
						currentFrame.X = 0;
					}
				}
			}
			// Ngược lại, cho bé ở tư thế tĩnh.
			else
			{
				currentFrame.X = 1;
				currentFrame.Y = (int)currentDirection;
			}
		}

		/// <summary>
		/// Bé bắt đầu chơi.
		/// </summary>
		public void Play()
		{
			currentCheckPoint = 0;
			this.IsPlaying = true;
		}

		/// <summary>
		/// Di chuyển bé đến tọa độ xác định.
		/// </summary>
		/// <param name="destination">Tọa độ Vector2.</param>
		public void MoveTo(Vector2 destination)
		{
			if (destination != position)
			{
				this.isMoving = true;
				position = Vector2.Clamp(position + MovingOffset,
					Vector2.Min(position, destination),
					Vector2.Max(position, destination));
			}
			else this.isMoving = false;
		}
		#endregion

		#region Auxiliary_Methods
		/// <summary>
		/// Lấy góc quay giữa 2 điểm.
		/// </summary>
		/// <param name="originCoordinate">Điểm đầu.</param>
		/// <param name="targetCoordinate">Điểm đích.</param>
		/// <returns></returns>
		protected double GetAngle
			(Vector2 originCoordinate, Vector2 targetCoordinate)
		{
			return GetAngle
				(originCoordinate.X, originCoordinate.Y,
				targetCoordinate.X, targetCoordinate.Y);
		}

		/// <summary>
		/// Lấy góc quay giữa 2 sprites.
		/// </summary>
		/// <param name="originSprite">Sprite đầu.</param>
		/// <param name="targetSprite">Sprite đích.</param>
		/// <returns></returns>
		protected double GetAngle(Sprite originSprite, Sprite targetSprite)
		{
			return GetAngle
				(originSprite.Position, targetSprite.Position);
		}

		/// <summary>
		/// Lấy góc quay giữa 2 điểm A và B với hoành độ, tung độ xác định.
		/// </summary>
		/// <param name="originX">Hoành độ điểm A.</param>
		/// <param name="originY">Tung độ điểm A.</param>
		/// <param name="targetX">Hoành độ điểm B.</param>
		/// <param name="targetY">Tung độ điểm B.</param>
		/// <returns></returns>
		protected double GetAngle
			(double originX, double originY, double targetX, double targetY)
		{
			double temp = Math.Sqrt
				((targetX - originX) * (targetX - originX)
				+
				(targetY - originY) * (targetY - originY));
			temp = MathHelper.Clamp
				((float)((targetX - originX) / temp), -1.0f, 1.0f);
			if (targetY > originY)
				return Math.Acos(temp);
			else
				return -Math.Acos(temp);
		}
		#endregion
	}
}
