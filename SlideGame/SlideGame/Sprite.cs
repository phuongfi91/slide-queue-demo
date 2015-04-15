using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SlideGame
{
	/// <summary>
	/// Sprite là class cơ bản. Đại diện cho các đối tượng trong game.
	/// </summary>
	public class Sprite
	{
		#region Declarations

		/************************************************************************/
		/*                      Texture (hoặc Sprite Sheet)                     */
		/************************************************************************/
		// Texture của Sprite. Texture này có thể đơn thuần là một file hình,
		// hoặc nó cũng có thể là một Sheet (Sprite Sheet), bao gồm nhiều hình
		// tạo thành một chuỗi chuyển động animation.
		protected Texture2D textureImage;
		//////////////////////////////////////////////////////////////////////////



		/************************************************************************/
		/*                  Các thông tin để sử dụng Sprite Sheet               */
		/************************************************************************/
		// Do Texture có thể là một Sprite Sheet chứa các hình nối tiếp nhau 
		// tạo thành một chuỗi chuyển động animation:

		// Sprite Sheet sẽ có kích thước sheetSize (Ví dụ: Sheet với 5 cột,
		// và 4 dòng sẽ có sheetSize là 5x4)
		protected Point sheetSize;

		// Mỗi Sprite trong Sheet sẽ có kích thước frameSize pixel AxB 
		// (Ví dụ: Sheet có sheetSize 5x4 và frameSize 64x64 thì tổng kích thước
		// sẽ là (5x64)x(4x64) = 320x256)
		protected Point frameSize;

		// currentFrame sẽ đại diện cho frame hiện tại đang được vẽ trong
		// chuỗi animation.
		protected Point currentFrame;

		// isLooped sẽ quyết định xem chuỗi animation này có lặp lại hay không.
		protected bool isLooped;

		// Nếu isLooped == false thì isFinishedAnimating sẽ xác định xem chuỗi
		// animation có kết thúc hay chưa.
		protected bool isFinishedAnimating;
		//////////////////////////////////////////////////////////////////////////



		/************************************************************************/
		/*                     Các thông số của đối tượng:                      */
		/************************************************************************/
		// position: tọa độ (Pixel) hiện tại của đối tượng (trong cả màn chơi)
		protected Vector2 position;

		// origin: trọng tâm của đối tượng. (Ví dụ: với frameSize là 64x64,
		// ta có thể set origin = 32x32 -> trọng tâm nằm ngay chính giữa đối
		// tượng). Đây là một thông số quan trọng liên quan đến việc xác định
		// tọa độ và khả năng xoay của đối tượng.
		protected Vector2 origin;

		// rotationAngle: góc quay của đối tượng.
		protected float rotationAngle;

		// scale: kích thước của đối tượng. Mặc định có giá trị 1.0f.
		// -> Tăng scale <=> Phóng lớn.
		// -> Giảm scale <=> Thu nhỏ.
		protected float scale;

		// drawLayer: do có rất nhiều đối tượng đc vẽ lên màn hình trong cùng
		// một lúc, phải có một chỉ số để xác định đối tượng nào nằm trên, đối
		// tượng nào nằm dưới, đó chính là drawLayer, có giá trị từ 0.0f -> 1.0f.
		protected float drawLayer;

		// Các thông số điều chỉnh tốc độ khung hình của 1 Sprite xác định
		// (Không phải tốc độ khung hình FPS của cả Game).
		// Nếu millisecondsPerFrame = defaultMillisecondsPerFrame = 16 thì
		// trong 1 giây, Sprite sẽ được vẽ khoảng 62 lần (1000 / 16 = 62.5)
		// -> millisecondsPerFrame càng lớn thì cử động animation của Sprite
		// càng chậm và ngược lại.
		protected int timeSinceLastFrame = 0;
		protected int millisecondsPerFrame;
		protected const int defaultMillisecondsPerFrame = 16;
		//////////////////////////////////////////////////////////////////////////
		#endregion

		#region Properties
		/// <summary>
		/// Truy xuất kích thước frameSize.
		/// </summary>
		public Point FrameSize
		{
			get { return frameSize; }
		}

		/// <summary>
		/// Truy xuất và thay đổi góc quay rotationAngle.
		/// </summary>
		public virtual float RotationAngle
		{
			get { return rotationAngle; }
			// Khi set góc quay, nó sẽ luôn được Wrap lại, để giá trị luôn
			// nằm trong khoảng từ -Pi -> Pi.
			set { rotationAngle = MathHelper.WrapAngle(value); }
		}

		/// <summary>
		/// Truy xuất và thay đổi vị trí position.
		/// </summary>
		public Vector2 Position
		{
			get { return position; }
			set { position = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Nếu ko truyền vào tham số millisecondsPerFrame thì sử dụng 
		/// defaultMillisecondsPerFrame = 16 (mặc định)
		/// </summary>
		public Sprite
			(
			Texture2D textureImage, Vector2 position, float rotationAngle,
			Point frameSize, Point sheetSize, float drawLayer
			)
			: this
			(
			textureImage, position, rotationAngle, frameSize, sheetSize,
			drawLayer, defaultMillisecondsPerFrame
			)
		{
			// Bỏ trống vì đã gọi Constructor bên dưới.
		}

		/// <summary>
		/// Ngược lại, sử dụng millisecondsPerFrame từ Constructor.
		/// </summary>
		public Sprite
			(
			Texture2D textureImage, Vector2 position, float rotationAngle,
			Point frameSize, Point sheetSize, float drawLayer, 
			int millisecondsPerFrame
			)
		{

			/************************************************************************/
			/*                 Truyền thông tin vào từ Constructor                  */
			/************************************************************************/
			this.textureImage = textureImage;
			this.position = position;
			this.rotationAngle = rotationAngle;
			this.frameSize = frameSize;
			this.sheetSize = sheetSize;
			this.drawLayer = drawLayer;
			this.millisecondsPerFrame = millisecondsPerFrame;
			this.isLooped = true;
			this.isFinishedAnimating = false;
			//////////////////////////////////////////////////////////////////////////



			/************************************************************************/
			/*                     Khởi tạo các thông tin khác                      */
			/************************************************************************/
			// Trọng tâm của đối tượng sẽ nằm ngay chính giữa đối tượng
			this.origin = new Vector2(frameSize.X / 2, frameSize.Y / 2);

			// scale mặc định = 1.0f.
			this.scale = 1.0f;
			//////////////////////////////////////////////////////////////////////////
		}
		#endregion

		#region Methods
		/// <summary>
		/// Update các giá trị, thông số của đối tượng.
		/// </summary>
		public virtual void Update(GameTime gameTime)
		{
			// Update currentFrame để Sprite chuyển động theo mô hình trong
			// Sprite Sheet, tạo thành 1 chuỗi animation. (currentFrame chạy
			// từ trái sáng phải, rồi xuống dòng, sau đó lặp lại cho đến khi
			// hết Sprite Sheet thì quay về dòng đầu tiên, bằng cách này ta
			// tạo ra chuỗi chuyển động khép kín, lặp vô tận)
			timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
			if (timeSinceLastFrame > millisecondsPerFrame)
			{
				timeSinceLastFrame = 0;
				++currentFrame.X;
				if (currentFrame.X >= sheetSize.X)
				{
					currentFrame.X = 0;
					++currentFrame.Y;
					if (currentFrame.Y >= sheetSize.Y)
						if (isLooped)
						{
							currentFrame.Y = 0;
						}
						else
						{
							isFinishedAnimating = true;
						}
				}
			}
		}

		/// <summary>
		/// Vẽ đối tượng dựa trên các thông số của nó.
		/// </summary>
		public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			// Vẽ đối tượng.
			spriteBatch.Draw
				(
				textureImage, position, new Rectangle
					(
					currentFrame.X * frameSize.X,
					currentFrame.Y * frameSize.Y,
					frameSize.X, frameSize.Y
					),
				Color.White, RotationAngle, origin, 1.0f,
				SpriteEffects.None, drawLayer
				);

			/*// Lấy tọa độ của bóng.
			Vector2 shadowPosition = new Vector2
				(position.X + 5.0f, position.Y + 5.0f);
			// Vẽ bóng của đối tượng.
			spriteBatch.Draw
				(
				textureImage, shadowPosition, new Rectangle
					(
					currentFrame.X * frameSize.X,
					currentFrame.Y * frameSize.Y,
					frameSize.X, frameSize.Y
					),
				new Color(0, 0, 0, 100), RotationAngle, origin, 1.0f,
				SpriteEffects.None, drawLayer - 0.05f
				);*/
		}
		#endregion
	}
}