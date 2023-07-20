using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MechGameHelpers;

public class OrdersManager : Node
{
    private List<OrderItem> _orders = new List<OrderItem>();

    [Export]
    private NodePath ordersUIPath;
    private OrdersUIControl _ordersUI;

    private List<OrderState> _finishedOrders = new List<OrderState>();

    [Export]
    private Vector2 orderAddTimeout = new Vector2(45f, 75f);

    [Export]
    private int maxOrderCount = 4;


    [Export]
    private float _baseTimeLimit = 80f;

    [Export]
    private float _cheeseTimeLimitBuff = 15f;

    [Export]
    private float _sauceTimeLimitBuff = 10f;

    [Export]
    private float _roniTimeLimitBuff = 15f;

    private float _orderAddCooldown = 3f;

    private Random _rng = new Random();

    private int _idCarry = 0;

    private int _rejectedPizzaCount = 0;
    public int RejectedPizzaCount => _rejectedPizzaCount;

    [Export]
    NodePath orderAudioPath;
    private AudioStreamPlayer _audioPlayer;

    [Export]
    AudioStream addOrderStream;

    [Export]
    AudioStream successOrderStream;

    [Export]
    AudioStream failOrderStream;

    private TimeSpan _levelCountdown;

    [Export]
    private NodePath endPanelPath;
    private WinPanelControl _endPanel;

    private LevelManager _levelManager;

    [Export]
    NodePath countdownUIPath;
    private Label _countdownUI;

    [Export]
    int roundLengthSeconds = 600;

    public override void _Ready()
    {
        _ordersUI = GetNode<OrdersUIControl>(ordersUIPath) ?? throw new NullReferenceException();
        _audioPlayer = GetNode<AudioStreamPlayer>(orderAudioPath) ?? throw new NullReferenceException();
        _levelCountdown = TimeSpan.FromSeconds(roundLengthSeconds);
        _endPanel = GetNode<WinPanelControl>(endPanelPath) ?? throw new NullReferenceException();
        _levelManager = LevelManager.MustGetNode(this);
        _countdownUI = GetNode<Label>(countdownUIPath) ?? throw new NullReferenceException();
    }

    public override void _Process(float delta)
    {
        for(var i=_orders.Count-1; i>=0; i--) {
            var order = _orders[i];
            order.state.currentTime += delta;
            if(order.state.currentTime >= order.state.timeLimit) {
                RemoveOrder(order, true);
            }

            order.ui.SetProgress(order.state.currentTime / order.state.timeLimit);
        }

        _orderAddCooldown -= delta;
        if(_orderAddCooldown <= 0 && _orders.Count < maxOrderCount) {
            GenerateOrder();
            _orderAddCooldown = RandomRangef(_rng, orderAddTimeout.x, orderAddTimeout.y);
        }

        _levelCountdown -= TimeSpan.FromSeconds(delta);
        var levelCountString = _levelCountdown.ToString(@"mm\:ss");
        if(_countdownUI.Text != levelCountString) {
            _countdownUI.Text = levelCountString;
        }
        if(_levelCountdown <= TimeSpan.Zero) {
            var complete = _finishedOrders.Count(o => !o.failed);
            var failed = _finishedOrders.Count(o => o.failed);
            _levelManager.IsOver = true;
            _endPanel.Show(complete, failed);
        }
    }

    public void _HandleDragonAtePizza(PizzaState pizza) {
        var matchingOrder = MatchPizzaToOrder(pizza);

        if(matchingOrder == null) {
            _rejectedPizzaCount++;
            //TODO indicate fail
            return;
        }

        RemoveOrder(matchingOrder, false);
    }

    public bool PizzaOrderValid(PizzaState pizza) {
        return MatchPizzaToOrder(pizza) != null;
    }

    private OrderItem MatchPizzaToOrder(PizzaState pizza) {
        if(pizza.cookedState != CookedState.Cooked) {
            return null;
        }
        
        var hasRonis = pizza.roniLevel >= 1f;
        var hasCheese = pizza.cheeseLevel >= 1f;
        var hasSauce = pizza.sauceLevel >= 1f;
        
        return _orders.OrderBy(o => o.state.timeLimit - o.state.currentTime).FirstOrDefault(o => {
            return o.state.roni == hasRonis && o.state.cheese == hasCheese & o.state.sauce == hasSauce;
        });
    }

    private void GenerateOrder() {
        var hasRoni = RandomBool(_rng);
        var hasSauce = true;
        var hasCheese = true;

        var timeLimit = _baseTimeLimit;
        if(hasSauce) {
            timeLimit += _sauceTimeLimitBuff;
        }
        if(hasCheese) {
            timeLimit += _cheeseTimeLimitBuff;
        }
        if(hasRoni) {
            timeLimit += _roniTimeLimitBuff;
        }

        var orderState = new OrderState {
            sauce = hasSauce,
            cheese = hasCheese,
            roni = hasRoni,
            failed = false,
            currentTime = 0,
            id = ++_idCarry,
            timeLimit = timeLimit,
        };

        var orderUI = _ordersUI.AddOrder(orderState);

        var orderItem = new OrderItem {
            state = orderState,
            ui = orderUI,
        };

        _orders.Add(orderItem);

        _audioPlayer.Stream = addOrderStream;
        _audioPlayer.Seek(0);
        _audioPlayer.Play();
    }

    private void RemoveOrder(OrderItem order, bool failed) {
        order.ui.EndOrder(failed);
        order.state.failed = failed;
        _finishedOrders.Add(order.state);
        _orders.Remove(order);


        _audioPlayer.Stream = failed ? failOrderStream : successOrderStream;
        _audioPlayer.Seek(0);
        _audioPlayer.Play();
    }

    private class OrderItem {
        public OrderCardControl ui;
        public OrderState state;
    }
}
