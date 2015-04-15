using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlideGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class SlideGame : Microsoft.Xna.Framework.Game
	{
		#region Fields
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		// Timer. Dùng để xác định khi nào thì bé kế tiếp được vào chơi.
		TimeSpan timer = TimeSpan.Zero;
		TimeSpan timeInterval = new TimeSpan(0, 0, 2);

		// Sprite Font.
		SpriteFont spriteFont;

		// Sprite cầu tuột.
		Sprite spriteSlide1;
		Sprite spriteSlide2;

		// Background
		Texture2D background;

		// Sprite hàng đợi.
		Sprite spriteNumbers;

		// Tổng số các bé.
		int childrenPopulation;

		// Tuổi tối đa của các bé.
		int childrenMaxAge;

		// List chứa tất cả các bé.
		List<Child> childrenList;

		// List các điểm mà bé đang chơi sẽ đi qua.
		public static List<Vector2> Checkpoints;

		// List chứa các bé đang chơi.
		public static List<Child> PlayersList;

		// Queue chứa các bé đang đứng chờ.
		public static Queue<Child> ChildrenQueue;
		#endregion

		#region Constructor
		public SlideGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.Window.Title = "Slide Game Demo - " +
				"By Phuong D. Nguyen and Chau D. Nguyen";
			this.IsMouseVisible = true;
		} 
		#endregion

		#region Main_Methods
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			//////////////////////////////////////////////////////////////////////////
			// Set số lượng bé và tuổi tối đa.
			// Khuyến cáo: Nên giữ nguyên số lượng bé ! Vì chương trình được
			// tối ưu cho việc trình diễn DEMO hàng đợi bằng đồ họa, không
			// dùng cho mục đích mở rộng.
			childrenPopulation = 10;
			childrenMaxAge = 9;

			// Set danh sách các điểm mà bé đang chơi sẽ đi qua.
			Checkpoints = new List<Vector2>();
			Checkpoints.Add(new Vector2(155, 285));
			Checkpoints.Add(new Vector2(208, 56));
			Checkpoints.Add(new Vector2(415, 290));
			Checkpoints.Add(new Vector2(655, 290));
			Checkpoints.Add(new Vector2(656, 376));

			// Load cầu tuột và khởi tạo một số thông số cần thiết.
			spriteSlide1 = new Sprite(
				Content.Load<Texture2D>(@"Textures\textureSlide1"), new Vector2(
					Window.ClientBounds.Width / 2 - 120,
					Window.ClientBounds.Height / 2 - 60), 0,
					new Point(300, 300), new Point(1, 1), 1.0f);
			spriteSlide2 = new Sprite(
				Content.Load<Texture2D>(@"Textures\textureSlide2"), new Vector2(
					Window.ClientBounds.Width / 2 - 120,
					Window.ClientBounds.Height / 2 - 60), 0,
					new Point(300, 300), new Point(1, 1), 0.5f);

			// Load dãy số hàng đợi và khởi tạo một số thông số cần thiết.
			spriteNumbers = new Sprite(
				Content.Load<Texture2D>(@"Textures\textureNumbers"), new Vector2(
					Window.ClientBounds.Width / 2,
					Window.ClientBounds.Height / 2 + 200), 0,
					new Point(576, 64), new Point(1, 1), 1.0f);

			// Khởi tạo danh sách các bé với tuổi tác Random.
			Random random = new Random(DateTime.Now.Millisecond);
			childrenList = new List<Child>();
			for (int i = 0; i < childrenPopulation; ++i)
			{
				childrenList.Add(new Child(
					Content.Load<Texture2D>(@"Textures\textureSheetChild"),
					Vector2.Zero, 0, new Point(60, 60), new Point(3, 4), 0.75f,
					random.Next(1, childrenMaxAge), 3, 100));
			}

			// Sort danh sách các bé theo tuổi. Nhỏ trước, lớn sau.
			childrenList.Sort(CompareChildrenByAge);

			// Cho danh sách các bé đã sort vào Queue.
			ChildrenQueue = new Queue<Child>();
			foreach (Child child in childrenList)
			{
				ChildrenQueue.Enqueue(child);
			}

			// Lấy bé đầu tiên ra khỏi hàng đợi và cho chơi.
			Child player = ChildrenQueue.Dequeue();
			player.Position = GetPositionInQueue(1) - new Vector2(0, 64f);
			player.Play();

			// Thêm bé này vào danh sách các bé đang chơi.
			PlayersList = new List<Child>();
			PlayersList.Add(player);

			// Set vị trí đứng trên hàng đợi cho các bé còn lại.
			int queueNumber = 1;
			foreach (Child child in ChildrenQueue)
			{
				child.Position = GetPositionInQueue(queueNumber++);
			}
			//////////////////////////////////////////////////////////////////////////

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			spriteFont = Content.Load<SpriteFont>(@"Fonts\font");
			background = Content.Load<Texture2D>(@"Textures\background");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// TODO: Add your update logic here
			//////////////////////////////////////////////////////////////////////////
			// Thoát DEMO.
			KeyboardState keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.Escape)) this.Exit();

			// Tăng giảm timeInterval bằng bàn phím.
			if (keyboardState.IsKeyDown(Keys.Right))
				timeInterval += new TimeSpan(0, 0, 0, 0, 50);
			else if (keyboardState.IsKeyDown(Keys.Left))
				timeInterval -= new TimeSpan(0, 0, 0, 0, 50);

			// Khóa timeInterval trong khoảng từ 1 -> 5 giây.
			if (timeInterval < new TimeSpan(0, 0, 0))
				timeInterval = new TimeSpan(0, 0, 0);
			else if (timeInterval > new TimeSpan(0, 0, 5)) 
				timeInterval = new TimeSpan(0, 0, 5);

			// Nếu timer > timeInterval thì bé kế tiếp được phép vào chơi.
			timer += gameTime.ElapsedGameTime;
			if (timer > timeInterval)
			{
				if (ChildrenQueue.Count != 0
					&&
					ChildrenQueue.Peek().Position == GetPositionInQueue(1))
				{
					// Tiến hành lấy bé đầu hàng ra khỏi hàng đợi.
					Child child = ChildrenQueue.Dequeue();
					if (!child.IsPlaying) child.Play();
					PlayersList.Add(child);
				}

				// Reset timer.
				timer = TimeSpan.Zero;
			}

			// Update các bé đang chơi.
			for (int i = 0; i < PlayersList.Count; ++i)
			{
				Child player = PlayersList[i];
				player.Update(gameTime);
				if (!PlayersList.Contains(player)) --i;
			}

			// Update các bé đang đứng trong hàng đợi.
			int queueNumber = 1;
			foreach (Child child in ChildrenQueue)
			{
				child.NextPosition = GetPositionInQueue(queueNumber++);
				child.Update(gameTime);
			}
			//////////////////////////////////////////////////////////////////////////

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// Màu nền.
			GraphicsDevice.Clear(Color.LightSeaGreen);

			// TODO: Add your drawing code here
			//////////////////////////////////////////////////////////////////////////
			spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

			// Vẽ hướng dẫn thoát DEMO.
			spriteBatch.DrawString(spriteFont, 
				"Press ESC to exit DEMO.\n" + 
				"Press -> to increase TI.\n" + 
				"Press <- to decrease TI."
				/*Mouse.GetState().X.ToString() + "," + Mouse.GetState().Y.ToString()*/,
				new Vector2(400, 30), Color.White, 0, Vector2.Zero, 
				1.0f, SpriteEffects.None, 1.0f);

			// Vẽ Time Interval (timeInterval)
			spriteBatch.DrawString(spriteFont,
				"Time Interval (TI):" + timeInterval.TotalSeconds.ToString("F2") + "s",
				new Vector2(400, 150), Color.White, 0, Vector2.Zero,
				1.0f, SpriteEffects.None, 1.0f);

			// Vẽ cầu tuột.
			spriteSlide1.Draw(gameTime, spriteBatch);
			spriteSlide2.Draw(gameTime, spriteBatch);

			// Vẽ background
			spriteBatch.Draw(background, Vector2.Zero, new Rectangle(0, 0, 800, 480),
				Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

			// Vẽ hàng đợi.
			spriteNumbers.Draw(gameTime, spriteBatch);

			// Vẽ các bé và hiển thị tuổi trên đầu mỗi bé.
			foreach (Child child in childrenList)
			{
				child.Draw(gameTime, spriteBatch);
				spriteBatch.DrawString
					(spriteFont, child.Age.ToString(), new Vector2
						(child.Position.X - 10, child.Position.Y - 64), 
					Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.8f);
			}

			spriteBatch.End();
			//////////////////////////////////////////////////////////////////////////

			base.Draw(gameTime);
		} 
		#endregion

		#region Auxiliary_Methods
		/// <summary>
		/// Lấy tọa độ đứng của bé trên hàng đợi.
		/// </summary>
		/// <param name="queueNumber">Lấy giá trị từ 1 đến 9.</param>
		/// <returns>Trả về tọa độ là một Vector2.</returns>
		private Vector2 GetPositionInQueue(int queueNumber)
		{
			int beginningX = (int)spriteNumbers.Position.X - 64 * 5;
			int beginningY = (int)spriteNumbers.Position.Y - 64;
			return new Vector2(beginningX + 64 * queueNumber, beginningY);
		}

		/// <summary>
		/// Hàm so sánh tuổi. Hỗ trợ sort tuổi trong danh sách các bé.
		/// </summary>
		/// <param name="childA">Bé A.</param>
		/// <param name="childB">Bé B.</param>
		/// <returns>Trả về giá trị so sánh kiểu int, dùng cho Quick Sort.
		/// </returns>
		private int CompareChildrenByAge(Child childA, Child childB)
		{
			if (childA.Age > childB.Age) return 1;
			else if (childA.Age < childB.Age) return -1;
			else return 0;
		} 
		#endregion
	}
}
