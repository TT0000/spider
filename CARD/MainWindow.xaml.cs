using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CARD
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<Image, Card> UCMailerIP = new Dictionary<Image, Card>(104);
        Dictionary<Card, Image> UCMailerPI = new Dictionary<Card, Image>(104);

        GameStackPosition gameBoxPosiHelper;//负责牌的位置计算
        UnfinishPosition openingPosiHelper;//负责实现发牌动画所需要的位置计算
        FinishPosition finishPosiHelper;//负责已完成牌堆的位置计算
        List<Image> signal;
  
        CardMaker CardMaker;
        PuckStackServer gameServer;

        int hasFinished = 0;
        int nextToSend = 5;//进行发牌时要获取的第X组牌
        int difficulty = 0;//困难选择
        int hasSendNum = 0;
        int[] finishedCardType;

        Point beginMousePosition;//触发鼠标移动事件的最初位置，
        Point movingPosition;//记录鼠标移动中的位置，实现移动效果
        Point beginImagePosition;//记录图片的当前位置
        //移动时所需要的变量
        bool hasDown;
        bool hasMove;
        double dertTop;
        double dertLeft;


        List<Card> lastFour;
        List<Card> moverP;
        List<Image> moverI;

        Storyboard storyboard;
        Storyboard finishboard;

        public MainWindow()
        {
            InitializeComponent();
            GameWay gameway = new GameWay();
            if (gameway.ShowDialog() == true)
            {
                int way = gameway.getChooseId();
                switch (way)
                {
                    case 1:
                        MyDifficultyChooser chooser = new MyDifficultyChooser();
                        if (chooser.ShowDialog() == true)
                        {
                            difficulty += chooser.GetChooseId();
                        }
                        if (difficulty == 0)
                        {
                            MessageBox.Show("怎么搞得，difficulty是0");
                            difficulty = 1;
                        }
                        initGame(difficulty);//进行分组、发牌等操作
                        break;
                    case 2:
                        OnOpen(null, null);
                        break;
                }
            }
        }

        /*
         * 首先是初始化牌局：发出4堆牌和4张，共44张，再发牌一堆，共54张。未完成5堆
         * 完成44张牌的布局，这44张牌已经关联了一个Card对象，只是它们的背景是Reverse.png
         * 使用前4堆牌以及4张。
         * */
        public void initGame(int type)
        {
            gameBoxPosiHelper = new GameStackPosition(30, 10, 85, 18);
            finishPosiHelper = new FinishPosition(530, 30);
            storyboard = new Storyboard();//发牌动画
            finishboard = new Storyboard();//收牌动画
            openingPosiHelper = new UnfinishPosition(530, 1030);
            finishedCardType = new int[8];
            signal = new List<Image>(6);
            putOpeningCard(0, 6);
            provideCards(type);
            Image Item;//图片的引用
            CardGroup group;//一组牌的引用
            Card Ptem;//Card对象
            double top;
            double left;

            for (int j = 0; j < 4; j++)
            {
                group = CardMaker.getCardGroup(j + 1);//获得第1组到第4组的牌
                for (int i = 0; i < 10; i++)
                {
                    Item = makeImage();
                    setEvent(Item);
                    Ptem = group.cards.ElementAt(i);
                    //将图片tem和一个Card对象之间建立双向关联
                    UCMailerIP.Add(Item, Ptem);
                    UCMailerPI.Add(Ptem, Item);

                    //首先获得该游戏堆中有多少张卡片，然后获得位置；而且必须先获得位置，然后入堆，入图
                    top = gameBoxPosiHelper.getNextUnShowedTop(gameServer.GetStack(i + 1).getcurrentNum());
                    left = gameBoxPosiHelper.getStackLeft(i + 1);//获得第一堆到第四堆的左边界

                    gameServer.GetStack(i + 1).addCard(Ptem, true);//将Card对象入游戏堆
                    addToCanvas(top, left, Item);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                top = gameBoxPosiHelper.getNextUnShowedTop(gameServer.GetStack(i + 1).getcurrentNum());
                left = gameBoxPosiHelper.getStackLeft(i + 1);
                Item = makeImage();
                setEvent(Item);
                Ptem = lastFour.ElementAt(i);
                UCMailerIP.Add(Item, Ptem);//建立联系
                UCMailerPI.Add(Ptem, Item);
                gameServer.GetStack(i + 1).addCard(Ptem, true);
                addToCanvas(top, left, Item);
            }//将4张图片与Card关联，为以后设置图片做准备
            Thread.Sleep(1000);//睡1s
            deal();//开局
        }
        /*
         * 向游戏界面增加一张图——已经和一个Card对象相关联
         * **/
        private void addToCanvas(double top, double left, Image card)
        {
            gameBox.Children.Add(card);
            Canvas.SetLeft(card, left);
            Canvas.SetTop(card, top);
        }

      //获得一个图片，这个图片有着确定的背景，使用在发牌过程。
         
        private Image makeImage(int id, int type)
        {
            Image myImage = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Pic/" + type + "_" + id + ".png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            myImage.Source = bi;
            myImage.Width = 85;
            myImage.Height = 100;
            return myImage;
        }

        //布局时需要的图片，这个图片使用reverse.png作为背景
        private Image makeImage()
        {
            Image myImage = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Pic/" + "reverse.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            myImage.Source = bi;
            myImage.Width = 85;
            myImage.Height = 100;
            return myImage;
        }
        private void setEvent(Image myImage)
        {
            myImage.MouseLeftButtonDown += Card_MouseLeftButtonDown;
            myImage.MouseLeftButtonUp += Card_MouseLeftButtonUp;
            myImage.MouseMove += Card_MouseMove;
        }

        //初始化gameServer，其中已经包含了卡片信息。
        private void provideCards(int type)
        {
            gameServer = new PuckStackServer();
            CardMaker = new CardMaker(type);
            lastFour = CardMaker.GetlastCards();
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 这里要记录鼠标移动事件开始的位置信息 
            Image firstmover = (Image)e.OriginalSource;
            beginMousePosition = e.GetPosition(gameBox);
            beginImagePosition = new Point(Canvas.GetLeft(firstmover), Canvas.GetTop(firstmover));
            firstmover.CaptureMouse();//捕获鼠标
            hasDown = true;
        }

        
         // 移动纸牌和判断是否可以移动纸牌
       
        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (hasDown)//点击后进行鼠标移动时执行事件处理
            {
                Image Imover = (Image)sender;
                Card Pmover = UCMailerIP[Imover];//获得触发鼠标移动事件的图片对应的Card元素

                hasMove = true;
                if (gameServer.canMove(Pmover))//可以移动
                {
                    moverI = getImages(Pmover);//获得所有要移动的图片，这些图片有着相同的偏移量
                    movingPosition = e.GetPosition(gameBox);
                    dertTop = movingPosition.Y - beginMousePosition.Y;
                    dertLeft = movingPosition.X - beginMousePosition.X;

                    int count = moverI.Count;
                    for (int i = 0; i < count; i++)
                    {
                        
                        Canvas.SetLeft(moverI.ElementAt(i), beginImagePosition.X + dertLeft);
                        Canvas.SetTop(moverI.ElementAt(i), beginImagePosition.Y + i * gameBoxPosiHelper.getMoveVertical() + dertTop);
                       
                    }
                }
            }
        }
        private void Card_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            /*
             判断纸牌是否可以入堆
             */
            hasDown = false;
            Image Imover = (Image)sender;
            Imover.ReleaseMouseCapture();
            if (hasMove)//如果已经经过move事件
            {
                Imover = (Image)sender;
                //Canvas.SetZIndex(Imover, 105);//将移动的纸牌设置为最上层显示
                Card mover = UCMailerIP[Imover];
                int stackNow = gameBoxPosiHelper.getStackNumber(e.GetPosition(gameBox).X);
                int count = moverI.Count;
                if (gameServer.canReceive(mover, stackNow))//在这里实现动画效果，可以接收
                {
                    //首先是离开原来的堆
                    int x = mover.stackNum;
                    gameServer.GetStack(mover.stackNum).move(mover.sequence - 1, moverI.Count);

                    scores.Content = (Convert.ToInt32(scores.Content) + moverP.Count);
                    times.Content = (Convert.ToInt32(times.Content) + 1);

                    checksetBackground(x);

                    gameServer.GetStack(stackNow).Receive(moverP);
                    moveIntoStack(moverI, stackNow);
                    if (checkFinished(stackNow))//如果接收后完成了一列，那么移除
                    {
                        doFinish(stackNow);//已经从界面中移出
                        CardStack stack = gameServer.GetStack(stackNow);
                        stack.move(stack.getcurrentNum() - 13, 13);
                        checksetBackground(stackNow);
                    }
                }
                else//不能移动，返回原处
                {
                    Image tem;
                    for (int i = 0; i < count; i++)
                    {
                        tem = moverI.ElementAt(i);
                        Canvas.SetZIndex(tem, UCMailerIP[tem].sequence);
                        Canvas.SetLeft(tem, beginImagePosition.X);
                        Canvas.SetTop(tem, beginImagePosition.Y + i * gameBoxPosiHelper.getMoveVertical());
                    }
                }
                moverI.Clear();//清空进行移动操作的图片
                hasMove = false;
            }
        }
        /*
         * 移走牌时显示下一张牌的背景图
         * **/
        private void checksetBackground(int x)
        {
            int nowTop = gameServer.GetStack(x).getcurrentNum() - 1;
            if (nowTop >= 0 && gameServer.GetStack(x).cards.ElementAt(nowTop).hide)
            {
                setBackground(gameServer.GetStack(x).cards.ElementAt(nowTop));
                gameServer.GetStack(x).updateUnshowedNum();
                gameServer.GetStack(x).cards.ElementAt(nowTop).hide = false;
            }
            else if (nowTop < 0)
            {
                //doSomething;
                addToCanvas(gameBoxPosiHelper.getNextUnShowedTop(0), gameBoxPosiHelper.getStackLeft(x), makeImage(0, 0));//添加一张表示空堆的背景图
            }
        }
        /*
         * 如果要实现撤回，该函数可以直接调用实现界面上的撤回
         * **/
        private void moveIntoStack(List<Image> mImage, int stackNum)
        {
            int count = mImage.Count;
            int currentNum = gameServer.GetStack(stackNum).getcurrentNum() - mImage.Count;
            int currentUnshowNum = gameServer.GetStack(stackNum).getUnshowedNum();
            double stackTop = gameBoxPosiHelper.getNextshowedTop(currentNum - currentUnshowNum, currentUnshowNum);
            double stackLeft = gameBoxPosiHelper.getStackLeft(stackNum);
            Image item;
            for (int i = 0; i < count; i++)
            {
                item = moverI.ElementAt(i);
                Canvas.SetZIndex(item, UCMailerIP[item].sequence);
                Canvas.SetLeft(item, stackLeft);
                Canvas.SetTop(item, stackTop + i * gameBoxPosiHelper.getMoveVertical());
            }
        }
        /*
         * 接收牌后是否已经完成一列
         * **/
        private bool checkFinished(int stackNum)
        {
            CardStack stack = gameServer.GetStack(stackNum);

            int checkIndex = stack.cards.Count - 1;

            if (stack.cards.ElementAt(checkIndex).getID() != 1)
            {
                return false;//第一张牌不是1，一定没有，返回即可
            }
            else
            {
                //从lastId-1向上检查，如果中断，返回false
                if (checkIndex - stack.getUnshowedNum() < 12)
                {
                    return false;//已经展示的图片小于13，一定没完成
                }
                else//已经展示了13张以上
                {
                    int type = stack.cards.ElementAt(checkIndex).getType();
                    checkIndex--;
                    for (int i = 2; i <= 13; i++)
                    {
                        if (stack.cards.ElementAt(checkIndex).getID() != i || type != stack.cards.ElementAt(checkIndex).getType())
                        {
                            return false;
                        }
                        checkIndex--;
                    }
                    return true;
                }
            }
        }

        private void Unfinished_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
             在这里将实现发牌的代码，一共可以发牌五次，发牌前要检查是否有空位。
             */
            if (!hasEmptySpace())
            {
                deal();//发牌函数
            }
        }
        /*
         * 开启一段动画
         * **/
        private Storyboard getStoryBoard(Image image, int stack, double top, double left)
        {
            Storyboard myStory = new Storyboard();
            int unshowedNum = gameServer.GetStack(stack).getUnshowedNum();
            DoubleAnimation topAni = getAnimation(top, gameBoxPosiHelper.getNextshowedTop(gameServer.GetStack(stack).getcurrentNum() - unshowedNum, unshowedNum));
            DoubleAnimation leftAni = getAnimation(left, gameBoxPosiHelper.getStackLeft(stack));

            topAni.Completed += (sender, e) =>
            {
                double currentTop = Canvas.GetTop(image);
                image.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetTop(image, currentTop);
            };
            leftAni.Completed += (sender, e) =>
            {
                double currentLeft = Canvas.GetLeft(image);
                image.BeginAnimation(Canvas.LeftProperty, null);
                Canvas.SetLeft(image, currentLeft);
            };

            Storyboard.SetTarget(topAni, image);
            Storyboard.SetTargetProperty(topAni, new PropertyPath(Canvas.TopProperty));
            myStory.Children.Add(topAni);

            Storyboard.SetTarget(leftAni, image);
            Storyboard.SetTargetProperty(leftAni, new PropertyPath(Canvas.LeftProperty));
            myStory.Children.Add(leftAni);

            myStory.Duration = topAni.Duration;
            myStory.Completed += Story_Completed;

            return myStory;
        }
        private Storyboard getStoryBoardForFinish(Image image, double left, double top)
        {
            Storyboard myStory = new Storyboard();

            DoubleAnimation topAni = getAnimation(Canvas.GetTop(image), top);
            DoubleAnimation leftAni = getAnimation(Canvas.GetLeft(image), left);

            Storyboard.SetTarget(topAni, image);
            Storyboard.SetTargetProperty(topAni, new PropertyPath(Canvas.TopProperty));
            myStory.Children.Add(topAni);

            Storyboard.SetTarget(leftAni, image);
            Storyboard.SetTargetProperty(leftAni, new PropertyPath(Canvas.LeftProperty));
            myStory.Children.Add(leftAni);

            myStory.Duration = topAni.Duration;
            myStory.Completed += MyStory_Completed;
            return storyboard;
        }
        /*
         * 动画结束后的处理函数——实现一些细节处理
         * **/
        private void MyStory_Completed(object sender, EventArgs e)
        {
            ClockGroup clockGroup = (ClockGroup)sender;
            Storyboard storyboard = (Storyboard)clockGroup.Timeline;
            storyboard.Stop();
        }

        private void Story_Completed(object sender, EventArgs e)
        {
            ClockGroup clockGroup = (ClockGroup)sender;
            Storyboard storyboard = (Storyboard)clockGroup.Timeline;
            storyboard.Stop();
        }
        /*
         * 检查是否可以发牌
         * **/
        private bool hasEmptySpace()
        {
            for (int i = 1; i < 11; i++)
            {
                if (gameServer.GetStack(i).getcurrentNum() == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /*
         * 得到动画
         * **/
        private DoubleAnimation getAnimation(double f, double t)
        {
            DoubleAnimation newAnimation = new DoubleAnimation();
            newAnimation.From = f;
            newAnimation.To = t;
            newAnimation.Duration = TimeSpan.FromMilliseconds(500);
            return newAnimation;
        }
        /*
         * 获得进行移动的图片，以便得到对应的Card对象
         * **/
        private List<Image> getImages(Card head)
        {
            int start = head.sequence;
            int end = gameServer.GetStack(head.stackNum).getcurrentNum();
            moverP = gameServer.GetStack(head.stackNum).GetCards(start, end);
            int border = end - start + 1;
            List<Image> images = new List<Image>(border);
            Image item;
            for (int i = 0; i < border; i++)
            {
                item = UCMailerPI[moverP.ElementAt(i)];
                Canvas.SetZIndex(item, 100 + i);
                images.Add(item);
            }
            return images;
        }
        /*
         * 如果要实现两种花色，这里要修改背景的设置方式。
         * **/
        private void setBackground(Card Card)//此时Card已经关联到一个Image上了，要做的是设置他的图
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Pic/" + Card.getType() + "_" + Card.getID() + ".png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            Image pImage = UCMailerPI[Card];
            pImage.Source = bi;
        }
        /*
         * 发牌函数——菜单发牌可以调用的函数
         * **/
        private void deal()
        {
            if (hasSendNum < 6)
            {
                Image iItem;
                Card pItem;
                CardGroup group = CardMaker.getCardGroup(nextToSend);

                for (int i = 0; i < 10; i++)
                {
                    pItem = group.cards.ElementAt(i);
                    iItem = makeImage(pItem.getID(), pItem.getType());
                    setEvent(iItem);

                    UCMailerIP.Add(iItem, pItem);
                    UCMailerPI.Add(pItem, iItem);

                    addToCanvas(openingPosiHelper.getTop(), openingPosiHelper.getLeft(), iItem);
                }
                //实现发牌动画

                for (int i = 1; i < 11; i++)
                {
                    storyboard = getStoryBoard(UCMailerPI[group.cards.ElementAt(i - 1)], i, openingPosiHelper.getTop(), openingPosiHelper.getLeft());
                    gameServer.GetStack(i).addCard(group.cards.ElementAt(i - 1), false);
                    Canvas.SetZIndex(UCMailerPI[group.cards.ElementAt(i - 1)], group.cards.ElementAt(i - 1).sequence);
                    //label.Content = "左边距：" + openingPosiHelper.getLeft() + " 上边距：" + openingPosiHelper.getTop();
                    storyboard.Begin();
                }
                //*/
                gameBox.Children.Remove(signal.ElementAt(nextToSend - 5));
                openingPosiHelper.updateNum();
                nextToSend++;
                hasSendNum++;
            }
            else
            {
                MessageBox.Show("您已无牌可发~");
            }

        }
        /*
         * 收牌动画
         * **/
        private void doFinish(int stackNum)
        {
            List<Card> finished = gameServer.GetStack(stackNum).cards;
            int cardIndex = finished.Count - 1;//获取第一张牌的索引
            DoubleAnimation top;     
            DoubleAnimation left;
            Image image;
            Card firstCard = finished.ElementAt(cardIndex);
            finishedCardType[hasFinished] = firstCard.getType();
            for (int i = 0; i < 13; i++)
            {
                image = UCMailerPI[finished.ElementAt(cardIndex)];
                top = getAnimation(Canvas.GetTop(image), finishPosiHelper.getTop());
                left = getAnimation(Canvas.GetLeft(image), finishPosiHelper.getNextPositionLeft());
                image.BeginAnimation(Canvas.LeftProperty, left);
                image.BeginAnimation(Canvas.TopProperty, top);
                cardIndex--;
            }
            cardIndex = finished.Count - 1;
            for (int i = 1; i < 13; i++)
            {
                gameBox.Children.Remove(UCMailerPI[finished.ElementAt(cardIndex)]);
                cardIndex--;
            }
            Canvas.SetZIndex(UCMailerPI[finished.ElementAt(cardIndex)], (80 - hasFinished));
            finishPosiHelper.updateFinishNum();
            hasFinished++;
        }
        private void putOpeningCard(int offset, int groupNum)
        {
            int top = 530;
            int left = 1030 + offset * 15;
            Image unfinished;
            for (int i = 0; i < groupNum; i++)
            {
                unfinished = makeImage();
                unfinished.MouseLeftButtonUp += Unfinished_MouseLeftButtonUp;
                Canvas.SetZIndex(unfinished, 80 - i);
                signal.Add(unfinished);
                addToCanvas(top, left, unfinished);
                left += 15;
            }
        }

        private void cleanData()
        {
            //清除所有数据，从0开始
            UCMailerIP.Clear();
            UCMailerPI.Clear();

            finishPosiHelper.clear();
            signal.Clear();
            CardMaker.clear();
            gameServer.clear();

            hasFinished = 0;
            nextToSend = 5;//进行发牌时要获取的第X组牌


            gameBox.Children.Clear();
            gameBox.Children.Add(scoreBoard);
            times.Content = 0;
            scores.Content = 500;

        }

        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            cleanData();
            difficulty = 0;
            MyDifficultyChooser chooser1 = new MyDifficultyChooser();
            if (chooser1.ShowDialog() == true)
            {
                difficulty += chooser1.GetChooseId();
            }
            if (difficulty == 0)
            {
                MessageBox.Show("怎么搞得，difficulty是0");
                difficulty = 1;
            }
            initGame(difficulty);//进行分组、发牌等操作
        }

        private void OnGameAgain(object sender, RoutedEventArgs e)
        {
            cleanData();
            initGame(difficulty);
        }
        private void OnOpen(object sender, RoutedEventArgs e)
        {
            //先清空Canvas
            gameBox.Children.Clear();
            gameBox.Children.Add(scoreBoard);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "(*.ss)|*.ss";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                String filePath = openFileDialog1.FileName;
                FileStream stream = new FileStream(filePath, FileMode.Open);
                IFormatter formatter = new BinaryFormatter();

                gameBoxPosiHelper = (GameStackPosition)formatter.Deserialize(stream);
                openingPosiHelper = (UnfinishPosition)formatter.Deserialize(stream);//负责实现发牌动画所需要的位置计算
                finishPosiHelper = (FinishPosition)formatter.Deserialize(stream);//负责已完成牌堆的位置计算
          
                CardMaker = (CardMaker)formatter.Deserialize(stream);
                gameServer = (PuckStackServer)formatter.Deserialize(stream);

                hasFinished = (int)formatter.Deserialize(stream);
                finishedCardType = (int[])formatter.Deserialize(stream);
                nextToSend = (int)formatter.Deserialize(stream);//进行发牌时要获取的第X组牌
                hasSendNum = (int)formatter.Deserialize(stream);
                difficulty = (int)formatter.Deserialize(stream);//这里需要传入用户的选择


                beginMousePosition = (Point)formatter.Deserialize(stream);//触发鼠标移动事件的最初位置，
                                                                          //                         //通过它一是实现移动代码，二是判断获得对应堆的位置
                movingPosition = (Point)formatter.Deserialize(stream);//记录鼠标移动中的位置，实现移动效果
                beginImagePosition = (Point)formatter.Deserialize(stream);//记录图片的当前位置

                hasDown = (bool)formatter.Deserialize(stream);
                hasMove = (bool)formatter.Deserialize(stream);
                dertTop = (double)formatter.Deserialize(stream);
                dertLeft = (double)formatter.Deserialize(stream);

                lastFour = (List<Card>)formatter.Deserialize(stream);

                times.Content = formatter.Deserialize(stream);
                scores.Content = formatter.Deserialize(stream);
                stream.Close();

                //按堆还原牌区
                //还原完成区
                //还原 UCMailerIP、UCMailerPI;
                Image comeImage;
                signal = new List<Image>(6);
                for (int i = 0; i < hasSendNum; i++)
                {
                    comeImage = makeImage();
                    signal.Add(comeImage);
                }
                putOpeningCard(hasSendNum, 6 - hasSendNum);//同时完成了signal的添加
                //接下来完成UCMailerIP以及UCMailerPI——在堆栈里的牌都在这两个字典里
                List<CardStack> allStack = gameServer.getAllStack();
                List<Card> allCard;
                CardStack comeStack;
                Card comeCard;

                int stackSize;
                int showedNum;
                double left;
                double top;
                for (int i = 0; i < 10; i++)//遍历十个游戏栈
                {
                    comeStack = allStack.ElementAt(i);
                    allCard = comeStack.getAllcard();
                    stackSize = allCard.Count;
                    left = gameBoxPosiHelper.getStackLeft(comeStack.getStackId());
                    showedNum = 0;
                    for (int j = 0; j < stackSize; j++)
                    {
                        comeCard = allCard.ElementAt(j);
                        if (comeCard.isHide())
                        {
                            top = gameBoxPosiHelper.getNextUnShowedTop(j);
                            comeImage = makeImage();

                        }
                        else
                        {
                            top = gameBoxPosiHelper.getNextshowedTop(showedNum, comeStack.getUnshowedNum());
                            comeImage = makeImage(comeCard.getID(), comeCard.getType());
                            showedNum++;
                        }
                        setEvent(comeImage);
                        addToCanvas(top, left, comeImage);
                        UCMailerIP.Add(comeImage, comeCard);
                        UCMailerPI.Add(comeCard, comeImage);
                    }
                }
                //还原收发牌动画
                storyboard = new Storyboard();//发牌动画
                finishboard = new Storyboard();//收牌动画
                //接下来还原完成区
                Image image;
                for (int i = 0; i < hasFinished; i++)
                {
                    int cardType = finishedCardType[i];
                    image = makeImage(13, cardType);
                    top = finishPosiHelper.getTop();
                    left = finishPosiHelper.getLeft() + 15 * i;
                    addToCanvas(top, left, image);
                }
            }
        }
        private void OnSave(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDg = new SaveFileDialog();
            saveDg.FileName = "MySpiderSolitaire.ss";
            saveDg.AddExtension = true;
            saveDg.RestoreDirectory = true;
            if (saveDg.ShowDialog() == true)
            {
         
                string filePath = saveDg.FileName;
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                IFormatter formatter = new BinaryFormatter();


                formatter.Serialize(stream, gameBoxPosiHelper);
                formatter.Serialize(stream, openingPosiHelper);//负责实现发牌动画所需要的位置计算
                formatter.Serialize(stream, finishPosiHelper);//负责已完成牌堆的位置计算
     
                formatter.Serialize(stream, CardMaker);
                formatter.Serialize(stream, gameServer);

                formatter.Serialize(stream, hasFinished);
                formatter.Serialize(stream, finishedCardType);
                formatter.Serialize(stream, nextToSend);//进行发牌时要获取的第X组牌
                formatter.Serialize(stream, hasSendNum);
                formatter.Serialize(stream, difficulty);//这里需要传入用户的选择


                formatter.Serialize(stream, beginMousePosition);//触发鼠标移动事件的最初位置，
                                                                //                         //通过它一是实现移动代码，二是判断获得对应堆的位置
                formatter.Serialize(stream, movingPosition);//记录鼠标移动中的位置，实现移动效果
                formatter.Serialize(stream, beginImagePosition);//记录图片的当前位置
                                                                //                         //移动时所需要的变量
                formatter.Serialize(stream, hasDown);
                formatter.Serialize(stream, hasMove);
                formatter.Serialize(stream, dertTop);
                formatter.Serialize(stream, dertLeft);


                formatter.Serialize(stream, lastFour);

                formatter.Serialize(stream, times.Content);
                formatter.Serialize(stream, scores.Content);

                stream.Close();

            }
        }
        private void OnDeal(object sender, RoutedEventArgs e)
        {
            if (!hasEmptySpace())
            {
                deal();//发牌函数
            }
        }
        private void OnExit(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("确定要退出吗?", "友好提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                //添加一些操作 
                this.Close();
            }

        }

    }
}
