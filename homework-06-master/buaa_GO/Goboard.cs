/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics; 

namespace Go_WinApp
{

	public enum StoneColor : byte
	{
		black = 0, white = 1
	}


	/**
	 * 两种颜色的棋子的枚举定义
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};

		int nSize;		                //棋盘的大小
		const int nBoardMargin = 10;	//棋盘边界大小
		int nCoodStart = 4;
		const int	nBoardOffset = 20;//防溢出
		int nEdgeLen = nBoardOffset + nBoardMargin;
		int nTotalGridWidth = 360 + 36;	//棋盘总大小
		int nUnitGridWidth = 22;		//单个格子所占像素
		int nSeq = 0;
		Rectangle rGrid;		    //网格棋盘
		StoneColor m_colorToPlay;   //当前颜色
		GoMove m_gmLastMove;	    //最近下的一个子
		Boolean bDrawMark;	        //颜色？
		Boolean m_fAnyKill;	        //标记有没有死子
		Spot [,] Grid;		        //格子二维数组
		Pen penGrid, penStoneW, penStoneB,penMarkW, penMarkB;
		Brush brStar, brBoard, brBlack, brWhite, m_brMark;
	
        //绘图类
        int nFFMove = 10;   //恢复10个子
        int nRewindMove = 10;

		GoTree	gameTree;

		/// <ZZZZZZZ>
		///    控件包括。。图中所示的
		/// </ZZZZZZZ>
		private System.ComponentModel.Container components;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

		public GoBoard(int nSize)
		{
			//
			// 初始化棋盘form
			//
			InitializeComponent();

			//
			// ZZZZ: ZZZ ZZZ ZZZZZZZZZZZ ZZZZ ZZZZZ ZZZZZZZZZZZZZZZZZZZ ZZZZ
			//

			this.nSize = nSize;  //ZZZZZZZZZ ZZZZ.

			m_colorToPlay = StoneColor.black;

			Grid = new Spot[nSize,nSize];
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
			penGrid = new Pen(Color.Brown, (float)0.5);
			penStoneW = new Pen(Color.WhiteSmoke, (float)1);
			penStoneB = new Pen(Color.Black,(float)1);
			penMarkW = new Pen(Color.Blue, (float) 1);
			penMarkB = new Pen(Color.Beige, (float) 1);

			brStar = new SolidBrush(Color.Black);
			brBoard = new SolidBrush(Color.Orange);
			brBlack = new SolidBrush(Color.Black);
			brWhite = new SolidBrush(Color.White);
			m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}

		/// <ZZZZZZZ>
		///    ZZZZZZZZ ZZZZZZ ZZZ ZZZZZZZZ ZZZZZZZ - ZZ ZZZ ZZZZZZ
		///    ZZZ ZZZZZZZZ ZZ ZZZZ ZZZZZZ ZZZZ ZZZ ZZZZ ZZZZZZ.
		/// </ZZZZZZZ>
		private void InitializeComponent()
		{
            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // layout构建
            // 
            this.Open.Location = new System.Drawing.Point(445, 88);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(56, 23);
            this.Open.TabIndex = 2;
            this.Open.Text = "open";
            this.Open.Click += new System.EventHandler(this.Open_Click);
            // 
            //...加监听器
            // 
            this.Save.Location = new System.Drawing.Point(509, 88);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(56, 23);
            this.Save.TabIndex = 3;
            this.Save.Text = "save";
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // 还是...加监听器
            // 
            this.Rewind.Location = new System.Drawing.Point(509, 56);
            this.Rewind.Name = "Rewind";
            this.Rewind.Size = new System.Drawing.Size(56, 23);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = "<<";
            this.Rewind.Click += new System.EventHandler(this.Rewind_Click);
            // 
            // 监听器。。
            // 
            // 
            this.Forward.Location = new System.Drawing.Point(445, 24);
            this.Forward.Name = "Forward";
            this.Forward.Size = new System.Drawing.Size(56, 23);
            this.Forward.TabIndex = 0;
            this.Forward.Text = ">";
            this.Forward.Click += new System.EventHandler(this.Forward_Click);
            // 
            // =_=
            // 
            this.Back.Location = new System.Drawing.Point(509, 24);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(56, 23);
            this.Back.TabIndex = 1;
            this.Back.Text = "<";
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // 监听器
            // 
            this.FForward.Location = new System.Drawing.Point(445, 56);
            this.FForward.Name = "FForward";
            this.FForward.Size = new System.Drawing.Size(56, 23);
            this.FForward.TabIndex = 4;
            this.FForward.Text = ">>";
            this.FForward.Click += new System.EventHandler(this.FForward_Click);
            // 
            // 监听器
            // 
            // 
            this.textBox1.Location = new System.Drawing.Point(447, 128);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(120, 311);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "please oepn a .sgf file to view, or just play on the board";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // 监听器
            // 
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(581, 478);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Name = "GoBoard";
            this.Text = "Go_WinForm";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.Click += new System.EventHandler(this.GoBoard_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected void textBox1_TextChanged (object sender, System.EventArgs e)
		{
			return;
		}

		private void PaintHandler(Object sender, PaintEventArgs e)
		{
			UpdateGoBoard(e);
		}

		protected void Save_Click (object sender, System.EventArgs e)
		{
			return;
		}

		protected void Open_Click (object sender, System.EventArgs e)
		{
			OpenFile();
			showGameInfo();
		}

		protected void Rewind_Click (object sender, System.EventArgs e)
		{
			gameTree.reset();
			resetBoard();
            showGameInfo();
		}

		protected void FForward_Click (object sender, System.EventArgs e)
		{
            if (gameTree != null)
            {
                int i = 0; 
                GoMove gm = null;
                for (gm = gameTree.doNext(); gm != null; gm = gameTree.doNext()) 
                {
                    playNext(ref gm);
                    if (i++ > nFFMove)
                        break; 
                }
            }
		}

		protected void Forward_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doNext();
			if (null != gm)
			{
				playNext(ref gm);
			}
		}

		private void showGameInfo()
		{
			//游戏状态的显示
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

		protected void Back_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doPrev();	//撤销
            if (null != gm)
            {
                playPrev(gm);
            }
            else
            {
                resetBoard();
                showGameInfo(); 
            }
		}

		Boolean onBoard(int x, int y) 
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}

		protected void GoBoard_Click (object sender, System.EventArgs e)
		{
			return;
		}

		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//用刷子在form上画下这个店
		private Boolean closeEnough(Point p, int x, int y)
		{
			if (x < rGrid.X+nUnitGridWidth*p.X-nUnitGridWidth/3 ||
				x > rGrid.X+nUnitGridWidth*p.X+nUnitGridWidth/3 ||
				y < rGrid.Y+nUnitGridWidth*p.Y-nUnitGridWidth/3 ||
				y > rGrid.Y+nUnitGridWidth*p.Y+nUnitGridWidth/3)
			{
				return false;
			}
			else 
				return true;
		}
       /*
        * 判断接近与否，在不在棋盘上
        */
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!onBoard(p.X, p.Y) || !closeEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].hasStone())
				return; //点在了外面

			//弱点在了棋盘上，根据缩点位置画点
			gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
			playNext(ref gmThisMove);
			gameTree.addMove(gmThisMove);
		}

		public void playNext(ref GoMove gm) 
		{
			Point p = gm.Point;
			m_colorToPlay = gm.Color;	//下一步咯

			//把label都去掉
			clearLabelsAndMarksOnBoard(); 
			
			if (m_gmLastMove != null)
				repaintOneSpotNow(m_gmLastMove.Point);

			bDrawMark = true;
			Grid[p.X,p.Y].setStone(gm.Color);
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++);
			//重新载入
			setLabelsOnBoard(gm);
			setMarksOnBoard(gm);
			
			doDeadGroup(nextTurn(m_colorToPlay));
			//处理没气了的棋子
			if (m_fAnyKill)
				appendDeadGroup(ref gm, nextTurn(m_colorToPlay));
			else 
			{
				doDeadGroup(m_colorToPlay);
				if (m_fAnyKill)
					appendDeadGroup(ref gm, m_colorToPlay);
			}
			m_fAnyKill = false;
			
			optRepaint();

			//重新显示
			m_colorToPlay = nextTurn(m_colorToPlay);
			
			//换颜色
			textBox1.Clear();
			textBox1.AppendText(gm.Comment);
		}

		private void appendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].setNoKilled();
					}
			gm.DeadGroup = a;
			gm.DeadGroupColor = c;
		}

		public void resetBoard()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].removeStone();
			m_gmLastMove = null;
			Invalidate(null);
		}

		/*
		 * 悔棋
		 */
        public void playPrev(GoMove gm)
        {
            int x = gm.Point.X;
            int y = gm.Point.Y;
            Grid[x, y].removeStone();
            m_gmLastMove = gameTree.peekPrev();
            if (m_gmLastMove != null)
            {
                Grid[m_gmLastMove.Point.X, m_gmLastMove.Point.Y].setUpdated();
            }
            if (gm.DeadGroup != null)
            {
                System.Collections.IEnumerator myEnumerator = gm.DeadGroup.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    Point p;
                    p = (Point)myEnumerator.Current;
                    Grid[p.X, p.Y].setNoKilled();
                    Grid[p.X, p.Y].setStone(nextTurn(gm.Color));
                }
            }            
            optRepaint();
            return;
        }


				
		
		Rectangle getUpdatedArea(int i, int j) 
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
            再度绘制
         * */
		private void optRepaint()
		{
			Rectangle r = new Rectangle(0,0,0,0);
			Region	re;

			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isUpdated()) 
					{
						r = getUpdatedArea(i,j);
						re = new Region(r);
						Invalidate(re);
					}
		}

        /*
         * 重画一个点及其周遭
         */
        void repaintOneSpotNow(Point p)
		{
			Grid[p.X, p.Y].setUpdated();
			bDrawMark = false;
			Rectangle r = getUpdatedArea(p.X, p.Y);
			Invalidate( new Region(r));
			Grid[p.X, p.Y].resetUpdated();
			bDrawMark = true;
		}

		//?
		void recordMove(Point p, StoneColor colorToPlay) 
		{
			Grid[p.X,p.Y].setStone(colorToPlay);
			// ??
			m_gmLastMove = new GoMove(p.X, p.Y, colorToPlay, nSeq++);
		}

		StoneColor nextTurn(StoneColor c) 
		{
			if (c == StoneColor.black)
				return StoneColor.white;
			else 
				return StoneColor.black;
		}

		/**
		 *	bury the dead stones in a group (same color). 
		 *	if a stone in one group is dead, the whole group is dead.
		*/
		void buryTheDead(int i, int j, StoneColor c) 
		{
			if (onBoard(i,j) && Grid[i,j].hasStone() && 
				Grid[i,j].color() == c) 
			{
				Grid[i,j].die();
				buryTheDead(i-1, j, c);
				buryTheDead(i+1, j, c);
				buryTheDead(i, j-1, c);
				buryTheDead(i, j+1, c);
			}
		}

		void cleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].clearScanned();
		}

        /**
         *统计死子 
         */
        void doDeadGroup(StoneColor c) 
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					if (Grid[i,j].hasStone() &&
						Grid[i,j].color() == c) 
					{
						if (calcLiberty(i,j,c) == 0)
						{
							buryTheDead(i,j,c);
							m_fAnyKill = true;
						}
						cleanScanStatus();
					}
		}


		/**
		 * 上课的时候写的算气
		 */
		int calcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // ZZZZZZZ	
			
			if (!onBoard(x,y))
				return 0;
			if (Grid[x,y].isScanned())
				return 0;

			if (Grid[x,y].hasStone()) 
			{
				if (Grid[x,y].color() == c) 
				{
					//ZZZ ZZZZZZZZZZ ZZZ ZZZZZZZ ZZZZZ.
					Grid[x,y].setScanned();
					lib += calcLiberty(x-1, y, c);
					lib += calcLiberty(x+1, y, c);
					lib += calcLiberty(x, y-1, c);
					lib += calcLiberty(x, y+1, c);
				} 
				else 
					return 0;
			} 
			else 
			{// ZZZZ ZZ ZZZZZ ZZZ ZZZZZZZZZ. 
				lib ++;
				Grid[x,y].setScanned();
			}

			return lib;
		}


        /**
         *  给上一次下的棋子反色标记
         */
        void markLastMove(Graphics g) 
		{
			Brush brMark;
			if (m_gmLastMove.Color == StoneColor.white)
				brMark = brBlack;
			else 
				brMark = brWhite;
			Point p = m_gmLastMove.Point;
			g.FillRectangle( brMark,
				rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth-1)/8,
				3, 3);
		}

		private void clearLabelsAndMarksOnBoard()
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasLabel())
						Grid[i,j].resetLabel();
					if (Grid[i,j].hasMark())
						Grid[i,j].resetMark();
				}

		}

		private void setLabelsOnBoard(GoMove gm)
		{
			short	nLabel = 0;
			Point p;
			if (null != gm.Labels)
			{
				int i = gm.Labels.Count;
				i = gm.Labels.Capacity;

				System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setLabel(++nLabel);
				}
			}
		}

		private void setMarksOnBoard(GoMove gm)
		{
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setMark();
				}
			}
		}

		private Point SwapXY(Point p)
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}

		private void DrawBoard(Graphics g)
		{
            //绘制棋盘的标度
			string[] strV= {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19"};
			string [] strH= {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

			Point p1 = new Point(nEdgeLen,nEdgeLen);
			Point p2 = new Point(nTotalGridWidth+nEdgeLen,nEdgeLen);
			g.FillRectangle(brBoard,nBoardOffset,nBoardOffset,nTotalGridWidth+nBoardOffset,nTotalGridWidth+nBoardOffset);
			for (int i=0;i<nSize; i++)
			{
				g.DrawString(strV[i],this.Font, brBlack, 0, nCoodStart+ nBoardOffset + nUnitGridWidth*i);
				g.DrawString(strH[i],this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth*i, 0);
				g.DrawLine(penGrid, p1, p2);
				g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

				p1.Y += nUnitGridWidth;
				p2.Y += nUnitGridWidth;
			}
			//ZZZZ ZZZ ZZZZ ZZ ZZZ ZZZZZ
			Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5);
			Pen penLow = new Pen(Color.Gray, (float)0.5);

			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset);
			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nBoardOffset+1, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset+1);
		}

		void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);
			
			//绘制中性点
			drawStars(e.Graphics);

            //绘制附件
			drawEverySpot(e.Graphics);
		}

		//=_=
		void drawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//=_=
		void  drawStars(Graphics g)
		{
			drawStar(g, 4, 4);
			drawStar(g, 4, 10);
			drawStar(g, 4, 16);
			drawStar(g, 10, 4);
			drawStar(g, 10, 10);
			drawStar(g, 10, 16);
			drawStar(g, 16, 4);
			drawStar(g, 16, 10);
			drawStar(g, 16, 16);
		}

		/**
		 * 画一个棋子
		 */
		void drawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.white)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}
        //以述
		void drawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//ZZZZZZZZ ZZZZZ.

			//ZZZZZ ZZZ ZZ
			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);

			g.DrawString(strLabels[nLabel],	//ZZZZZZ ZZZZZZ ZZZZ 1, ZZZ ZZZ ZZZZZZ ZZZZZZ ZZZZ 0.
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}

		void drawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}

		void drawEverySpot(Graphics g) 
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasStone())
						drawStone(g, i, j, Grid[i,j].color());
					if (Grid[i,j].hasLabel())
						drawLabel(g, i, j, Grid[i,j].getLabel());
					if (Grid[i,j].hasMark())
						drawMark(g, i, j);
				}
			//ZZZZZZZZZ ZZZZ ZZZZ. 
			if (bDrawMark && m_gmLastMove != null)
				markLastMove(g);
		}

		//ZZZZ Z ZZZZ ZZZZ
		private void OpenFile()
		{
			OpenFileDialog openDlg = new OpenFileDialog();
			openDlg.Filter  = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
			openDlg.FileName = "" ;
			openDlg.DefaultExt = ".sgf";
			openDlg.CheckFileExists = true;
			openDlg.CheckPathExists = true;
			
			DialogResult res = openDlg.ShowDialog ();
			
			if(res == DialogResult.OK)
			{
				if( !(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF")) 
					MessageBox.Show("Unexpected file format","Super Go Format",MessageBoxButtons.OK);
				else
				{
					FileStream f = new FileStream(openDlg.FileName, FileMode.Open); 
					StreamReader r = new StreamReader(f);
					string s = r.ReadToEnd();
					gameTree = new GoTree(s);
					gameTree.reset();
                    resetBoard();
					r.Close(); 
					f.Close();
				}
			}		
		}	
	}

	public class GoTest
	{
		/// <ZZZZZZZ>
		/// 开始执行
		/// </ZZZZZZZ>
        [STAThread]
		public static void Main(string[] args) 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//棋盘点类
	public class Spot 
	{
		private Boolean bEmpty;
		private Boolean bKilled;
		private Stone s;
		private short	m_nLabel;
		private Boolean m_bMark;
		private Boolean bScanned;
        private Boolean bUpdated; //标记是否应该重画
		/**
		 * ZZZZZZZZZZZ.
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		
		public Boolean hasStone() { return !bEmpty;	}
		public Boolean isEmpty() {	return bEmpty;	}
		public Stone thisStone() {	return s;}
		public StoneColor color() {	return s.color;}

		public Boolean hasLabel() {return m_nLabel>0;}
		public Boolean hasMark() {return m_bMark;}
		public void setLabel(short l) {m_nLabel = l; bUpdated = true; }
		public void setMark() {m_bMark = true; bUpdated = true;}
		public void resetLabel() {m_nLabel = 0; bUpdated = true;}
		public void resetMark() {m_bMark = false; bUpdated = true;}
		public short	getLabel() {return m_nLabel;}

		public Boolean isScanned() { return bScanned;}
		public void setScanned() {	bScanned = true;}
		public void clearScanned() { bScanned = false; }

		public void setStone(StoneColor c) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s.color = c;
				bUpdated = true;
            } //设置当前棋盘点的颜色为c,标记其非空且已更新. 
		}

		/*
		 * 去子 Z ZZZZZ ZZZZ ZZZ ZZZZZZZZ
		*/
		public void removeStone()
		{	//已更新，且为空
			bEmpty = true;
			bUpdated = true;
		}
				
		//死子
		public void die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean isKilled() { return bKilled;}
		public void setNoKilled() { bKilled = false;}

		public void resetUpdated() { bUpdated = false; bKilled = false;}

		//属性
		public Boolean isUpdated() 
		{ 
			if (bUpdated)
			{	//ZZZZ ZZ ZZZZ ZZZ ZZZZZZ ZZZZZZZZZ ZZZ ZZZ ZZZZ ZZZZZZ
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		// ZZZZZ Z ZZZZ ZZ ZZ ZZZZZZZZZ, ZZZZ ZZ ZZZZZZZZ ZZZ ZZZZZ ZZZZ.
		public void setUpdated() { bUpdated = true; }
	}

	/**
	 * move类，就是这一步
	 */
	public class GoMove 
	{
		StoneColor m_c;	//颜色
		Point m_pos;		//location
		int m_n;			//第几部
		String m_comment;	//注释
        MoveResult m_mr;	//标记下完这一步之后周遭是否有被杀的棋子

		ArrayList		m_alLabel; //ZZZ ZZZZZ ZZ ZZZZ ZZZZ. 
		ArrayList		m_alMark; //ZZZZ

		//ZZZ ZZZZZ ZZ ZZZZ ZZZZZZ ZZZZZZ ZZ ZZZZ ZZZZ
		//ZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZZ (ZZ ZZZ ZZZZZZ ZZZZ ZZZZ ZZ ZZZZZZZ). 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * ZZZZZZZZZZZ.
		 */
		public GoMove(int x, int y, StoneColor sc, int seq) 
		{
			m_pos = new Point(x,y);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

		public GoMove(String str, StoneColor c) 
		{
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ - 
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = c;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}


		private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ - 
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}


        public StoneColor Color
        { 
            get { return m_c; } 
        }

        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }
		
		public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void addLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void addMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/**
	 * ZZZZZZZZZZ - ZZZ ZZZZZZ ZZ ZZZ 4 ZZZZZZZZZZZ ZZZZZZZZZ ZZZZ Z ZZZZ ZZ ZZZZZZ.
	 * 
	 */
	public class MoveResult 
	{
		public StoneColor color;
        // 4个方向. 
        public Boolean bUpKilled;
		public Boolean bDownKilled;
		public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	//ZZ ZZZ ZZZZ Z ZZZZZZZ?
		public MoveResult() 
		{
			bUpKilled = false;
			bDownKilled = false;
			bLeftKilled = false;
			bRightKilled = false;
			bSuicide = false;
		}
	}

    /**
     * 棋子类. 
     * ZZZZZ ZZZ ZZZ ZZZZZZZZ ZZZZZ, ZZZZZ ZZZ ZZZZZ. 
     */
    public struct Stone 
	{
		public StoneColor color; 
	}

    /**
     * Z 	记录类

     * ZZZZZZZZZZ: ZZZ ZZZZ ZZZZ ZZZZZZ ZZZ ZZ 0. 
     */
    public class GoVariation 
	{
		int m_id;			//ZZZZZZZZZ ZZ. 
		string m_name;	//ZZZZZZZZZ ZZZZ. (ZZZZ.5, ZZZ.9, "ZZZZZ ZZZZZZ", ZZZ).
		//ZZZZZZZZZZZZZ ZZZ;	//ZZZZZZZZZ ZZZZZZZZ ZZZZZ.	
		ArrayList m_moves; 
		int m_seq;			//ZZZZZZ ZZZ ZZZ ZZ ZZZZ ZZZZ. 
		int m_total;

		//ZZZZZZZZZZZ. 
		public GoVariation(int id)
		{
			m_id = id;
			m_moves = new ArrayList(10);
			m_seq = 0;
			m_total = 0;
		}

		public void addAMove(GoMove gm) 
		{
			gm.Seq = m_total ++;
			m_seq++;
			m_moves.Add(gm);
		}

		public void updateResult(GoMove gm) 
		{
		}

		public GoMove doNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[m_seq++];
			} 
			else 
				return null;
		}

		public GoMove doPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/*
		 *  ZZZZ ZZZZZZ ZZZ ZZZZZZZZ ZZZZ, ZZ ZZZZZZ ZZZZZZ ZZ ZZZ ZZZZZZZZ.
		 */
		public GoMove peekPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[m_seq-1]);
			else 
				return null;
		}

		public void reset() {m_seq = 0;}
	}


	/**
	* ZZ: ZZZ ZZ ZZ Z ZZZZZZZZZ ZZZZZ ZZZZ ZZZ ZZZZZZZ ZZZZ. 
	* ZZZ: ZZZ ？？？？？ ZZ Z ZZZZZZZZZ ZZZZZ ZZZZZZZ ZZZZ. 
	*/
	struct VarStartPoint
	{
		int m_id; 
		int m_seq;
	}

	struct GameInfo 
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             //游戏时间
        public string unknown_ff;   //!!!
        public string unknown_gm;
        public string unknown_vw; 
	}

	public class KeyValuePair 
	{
		public string k; public ArrayList alV;

		private string	removeBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
                if (strOut[iSlash] == '\\')		// && strOut[iSlash+1] == ']')
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
                        iSlash--;	//ZZZZ ZZ ZZZZ ZZZZZZZZZ ZZZZZ ZZ ZZZZZZ ZZZ slash处理转义字符
				}
			}
			return strOut;
		}

		public KeyValuePair(string k, string v)
		{
			this.k = string.Copy(k);
			string strOneVal;
			int		iBegin, iEnd;
		
			//ZZZZ ZZ ZZZZZ ZZZZ ZZZZZ
			alV = new ArrayList(1);

			//ZZZZZZZ ZZZZ ZZZ ZZZZZZZ Z[ZZZZZZZ]
			if (k.Equals("C"))
			{
				strOneVal = removeBackSlash(string.Copy(v));
				//ZZZ ZZZ ZZ '\'
				alV.Add(strOneVal);
				return;
			}

			iBegin = v.IndexOf("[");
			if (iBegin == -1)	//ZZZZZZ ZZZZZ
			{
				alV.Add(v);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < v.Length && iBegin>=0)
			{
				iEnd = v.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = v.Substring(iBegin, iEnd-iBegin);
				else 
					strOneVal = v.Substring(iBegin);	//ZZZ ZZZZ ZZZZZ
				alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
					iBegin ++;	//ZZZ ZZ ZZZ ZZZZZ ZZ ZZZZ ZZZZZ
			}
		}
	}

    /**
     * 历史信息树
     */

	public class GoTree 
	{
		GameInfo _gi;		//ZZZZZ ZZZ ZZZZ'Z ZZZZZZZ ZZZZ.
		ArrayList _vars;		//ZZZZZZZZZZ. 
		int _currVarId;		//ZZ ZZ ZZZZZZZ ZZZZZZZZZ.
		int _currVarNum;
		GoVariation _currVar;		//ZZZZZZZ ZZZZZZZZZZZ.
		string	_stGameComment;

		// ZZZZZZZZZZZ - ZZZZZZ ZZZ ZZZZZZ ZZZZZ ZZ ZZZZZ ZZZZZZ
		public GoTree(string s)
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			parseFile(s);
		}

		//	ZZZZZZZZZZZ - ZZZZZZ ZZ ZZZZZ ZZZZZZ
		public GoTree()
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void addMove(GoMove gm) 
		{
			_currVar.addAMove(gm);
		}

		/**
		 * 分析文件
		 */
		Boolean parseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);
				iEnd = goStr.IndexOf(";", iBeg+1);
				if (iEnd < 0) //ZZ ZZZZ ";"
					iEnd = goStr.LastIndexOf(")", goStr.Length);		//ZZZ ZZZZ ZZZZZZZ ZZZZZZ ZZ ZZZZZZZZ ZZ ")"
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					parseASection(section);
				} 
				else 
					break;
			}
			return true;
		}

        /// <ZZZZZZZ>
        //XML吧大概. 
        /// </ZZZZZZZ>
        /// <ZZZZZ ZZZZ="ZZZ"></ZZZZZ>
        /// <ZZZZZZZ></ZZZZZZZ>
        int findEndofValueStr(String sec)
        {
            int i = 0;
            //ZZ ZZZZZZ ZZ'ZZ ZZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZ ZZZZZZ.
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    //ZZZZZZ ZZZ ZZZZZ ZZ "]". 
            }

            //ZZ ZZ ZZZZ ZZ ZZZ ']', ZZZ'Z ZZZZ ZZZ ZZZ ZZZ ZZ ZZZZZZ. 
            return sec.Length - 1;		//ZZZZ ZZ ZZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZ ZZ ZZZ ZZZZZZ
        }
        
        int findEndofValueStrOld(String sec)
		{
			int i = 0;
            //ZZ ZZZZZZ ZZ'ZZ ZZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZ ZZZZZZ. 
			bool fOutside = false;
			
			for (i=0; i<sec.Length;i++)
			{
				if (sec[i] == ']')
				{
					if (i>1 && sec[i-1] != '\\') //ZZ ZZ
						fOutside = true;
				}
				else if (char.IsLetter(sec[i]) && fOutside && i>0)
					return i-1;
				else if (fOutside && sec[i] == '[')
					fOutside = false;
			}
			return sec.Length-1;		//ZZZZ ZZ ZZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZ ZZ ZZZ ZZZZZZ
		}

        private string purgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; //ZZZZZ ZZ ZZZ ZZ ZZZZZZ. 

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast+1);  //ZZZ 2ZZ ZZZZZZZZZ ZZ ZZZ ZZZZZZ
            else
                return inStr; 
        }
 

		/**
		 * ZZZZZ Z ZZZZZZZ ZZ ZZZ ZZZZZZ ZZZZZZ. 
		 * Z ZZZZZZZ ZZZ ZZZ ZZZZZZ "ZZ {ZZ}"
		 * Z ZZ (ZZZ ZZZZZ ZZZZ) ZZZ ZZZ ZZZZZZ "ZZZ ZZZZZ {ZZZZZ}"
		 * ZZZZ: Z ZZZ ZZZ ZZZZZZZZZ ZZZZ ZZZZZZZZ ZZZZZZ, Z.Z. ZZZZZZ, ZZZZZ:  Z[ZZ][ZZ]. 
		 * Z ZZZ ZZ ZZZZZZ 
		 * Z ZZZZZ ZZ Z ZZZZZZ ZZZZZZZZ ZZ [ ZZZ ].
		 * ZZZZ: ZZZZZZZZ ( Z[ZZZZZZZZ]) ZZZ ZZZZ ZZZ ']' ZZZZZZZZZ ZZZZZZ ZZZ ZZZZZZZZ, ZZZZZ ZZ ZZZZZZZ ZZ "\]"
		 * Z.Z.  Z[ZZZZZ ZZZZZ ZZ [4,Z\] ZZ ZZZZZ ZZZZZZ]
         * 
		 */
		Boolean parseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
			try 
			{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = purgeCRLFSuffix(sec);
 
				iValue = findEndofValueStr(sec); //ZZZ.ZZZZZZZ("]", ZZZZ);
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
				while (iKey > 0 && iValue > iKey)//ZZ ZZZZ ZZZZZ ZZ ZZZZZZZ
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
					//ZZZZ ZZ ZZZZ ZZZ ZZZZ ZZZZZZ ZZZZZZZ ZZ Z [] ZZZZ
					//ZZZZZZ = ZZZZZZZZZZZZZZZZZ(ZZZ);
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
					//ZZZ ZZ ZZZZ Z ZZZ/ZZZZZ ZZZZ
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
						iValue = findEndofValueStr(sec); //ZZZ.ZZZZZZZ("]",ZZZZ);
					}
				}
			}
			catch
			{
                return false;
            }

			processASection(Section);
			return true;
		}


        /** 
         * ZZZZZZZ Z ZZZ ZZZ ZZZ ZZZZZZZZZZZZZ ZZZZZ
         * ZZ ZZZZZ ZZ Z ZZZZ, ZZ ZZZZZZ ZZZZZZZZZZZ.
         * ZZZZZZZZZZ, ZZZ ZZZZZZZ ZZZ ZZZZ ZZZZ ZZZ ZZZZ ZZ ZZZZ. 
         * 
         * ZZZZ: ZZ/ZZ ZZZ ZZZZZZZZZ ZZZ ZZZZ ZZZZZZZZ ZZZZZ ZZZZZZ, ZZZ Z ZZZZZ'Z ZZZZ ZZZ ZZZZZZZ ZZZ 
         */
        Boolean processASection(ArrayList arrKV) 
		{
			Boolean fMultipleMoves = false;   //ZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZZZZ ZZZZZ. 
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).k;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   //ZZZZZ ZZZZZ
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("W"))  //ZZZZZ ZZZZZ
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("C"))  //ZZZZZZZ
                    {
                        //ZZZZZ.ZZZZZZ(Z>0);
                        if (gm != null)
                            gm.Comment = strValue;
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  //ZZZZZ
                    {
                        if (gm != null)
                            gm.addLabel(strValue);
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M"))  //ZZZZ
                    {
                        if (gm != null)
                            gm.addMark(strValue);
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		//ZZZ ZZZZZ ZZZZZ
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("AB"))		//ZZZ ZZZZZ ZZZZZ
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("HA"))
                        _gi.handicap = (strValue);
                    else if (key.Equals("BR"))
                        _gi.rankBlack = (strValue);
                    else if (key.Equals("PB"))
                        _gi.playerBlack = (strValue);
                    else if (key.Equals("PW"))
                        _gi.playerWhite = (strValue);
                    else if (key.Equals("WR"))
                        _gi.rankWhite = (strValue);
                    else if (key.Equals("DT"))
                        _gi.date = (strValue);
                    else if (key.Equals("KM"))
                        _gi.km = (strValue);
                    else if (key.Equals("RE"))
                        _gi.result = (strValue);
                    else if (key.Equals("SZ"))
                        _gi.size = (strValue);
                    else if (key.Equals("EV"))
                        _gi.gameEvent = (strValue);
                    else if (key.Equals("PC"))
                        _gi.location = (strValue);
                    else if (key.Equals("TM"))
                        _gi.time = (strValue);
                    else if (key.Equals("GN"))
                        _gi.gameName = strValue;

                    else if (key.Equals("FF"))
                        _gi.unknown_ff = (strValue);
                    else if (key.Equals("GM"))
                        _gi.unknown_gm = (strValue);
                    else if (key.Equals("VW"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("US"))
                        _gi.unknown_vw = (strValue);

                    else if (key.Equals("BS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("WS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("ID"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("KI"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("SO"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("TR"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("LB"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("RO"))
                        _gi.unknown_vw = (strValue);


                    //ZZZZ ZZZZZ
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " "+ strValue);

                    //ZZZZZZZZZ ZZZ ZZZZ ZZZZZZ ZZZ ZZZZ ZZ ZZZZ ZZZZ (ZZ, ZZ) ZZZZ ZZZZZZZZ ZZZZZ. 
                    if (fMultipleMoves)
                    {
                        _currVar.addAMove(gm);
                    }
                }
            }

            //ZZZ ZZZ ZZZZ ZZ ZZZZZZZ ZZZZZZZZZ. 
            if (!fMultipleMoves && gm != null)
            {
                _currVar.addAMove(gm);
            }
			return true;
		} 

		public GoMove doPrev() 
		{
			return _currVar.doPrev();
		}

		public GoMove peekPrev() 
		{
			return _currVar.peekPrev();
		}

		public GoMove doNext() 
		{
			return _currVar.doNext();
		}

		public void updateResult(GoMove gm) 
		{
			_currVar.updateResult(gm);
		}
		
		public void reset()
		{
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar.reset();
		}
		public void rewindToStart()
		{

		}
	} //？不清楚
}
