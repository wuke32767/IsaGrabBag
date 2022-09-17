// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.IO;
// using Microsoft.Xna.Framework;
// using Monocle;
// using NLua;

// namespace Celeste.Mod.IsaGrabBag
// {
// 	static class LuaEXT {
// 		public static bool ContainsKey(this LuaTable _table, object _key) {
// 			foreach (var obj in _table.Keys)
// 				if (obj == _key)
// 					return true;

// 			return false;
// 		}
// 		public static bool ContainsValue(this LuaTable _table, object _value) {
// 			foreach (var obj in _table.Values)
// 				if (obj == _value)
// 					return true;

// 			return false;
// 		}
// 		public static int Count(this LuaTable _table) {
// 			int count = 0;
// 			foreach (var obj in _table.Keys)
// 				count++;

// 			return count;
// 		}
// 		public static int Integer(this LuaTable _table, object _key, int _default = 0) {
// 			var obj = _table[_key];
// 			if (obj == null)
// 				return _default;

// 			switch (obj) {
// 				case long l:
// 					return (int)l;
// 				case double d:
// 					return (int)d;
// 				default:
// 					return _default;
// 			}
// 		}
// 		public static float Float(this LuaTable _table, object _key, float _default = 0) {
// 			var obj = _table[_key];
// 			if (obj == null)
// 				return _default;

// 			switch (obj) {
// 				case long l:
// 					return (float)l;
// 				case double d:
// 					return (float)d;
// 				default:
// 					return _default;
// 			}
// 		}
// 	}
// 	public class CustomPlayer : Actor {
// 		public static void LeaveLevel(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
// 			instance = null;
// 		}

// 		public static CustomPlayer instance { get; private set; }

// 		public static CustomPlayer GetPlayer() {
// 			return instance;
// 		}

// 		public Vector2 Speed;
// 		public float CoyoteTime;

// 		public string PlayerType { get; private set; }
// 		LuaTable functions;
// 		StateMachine machine;

// 		private Entity playerFollower;

// 		private string[] stateNames;

// 		private int updateErrorWait;

// 		public CustomPlayer(EntityData _data, Vector2 offset) : base(_data.Position + offset) {

// 			PlayerType = _data.Attr("entity", "IsaExample");
// 			string path = Path.Combine("LuaPlayer", PlayerType, "main");

// 			var cont = Everest.Content.Get(path);

// 			if (cont == null)
// 				return;

// 			try {
// 				functions = Everest.LuaLoader.Context.DoString(cont.Data)[0] as LuaTable;

// 				bool start = instance == null;

// 				if (instance == null)
// 					instance = this;

// 				instance.InitCharacter(functions, start);

// 				Tag = Tags.Persistent;
// 			}
// 			catch (Exception e) {
// 				Logger.Log("Lua Player", $"Error Loading player module : {e}");
// 				instance = null;
// 				return;
// 			}

// 		}
// 		private void InitCharacter(LuaTable code, bool isInit) {

// 			var initFunc = code["init"] as LuaFunction;

// 			if (initFunc != null)
// 				initFunc.Call(isInit);

// 			Collider = new Hitbox(8, 8);
// 			functions = code;

// 			if (!isInit) {
// 				Remove(machine);
// 			}

// 			foreach (var obj in code.Keys) {
// 				Logger.Log("Lua Character", $"\"{obj}\" : \"{code[obj].GetType()}\"");
// 			}

// 			var array = code["states"] as LuaTable;

// 			Add(machine = new StateMachine(array.Keys.Count));
// 			stateNames = new string[array.Keys.Count];
// 			int index = 0;

// 			foreach (var key in array.Keys) {

// 				stateNames[index] = array[key] as string;
// 				machine.ReflectState(this, index, "machine");
// 				index++;
// 			}

// 		}

// 		public override void Awake(Scene scene) {

// 			if (instance != this) {
// 				if (instance == null) {

// 					GrabBagModule.playerInstance.Active = true;
// 					GrabBagModule.playerInstance.Visible = true;
// 					GrabBagModule.playerInstance.Collidable = true;
// 				}
// 				RemoveSelf();
// 				return;
// 			}

// 			GrabBagModule.playerInstance.Active = false;
// 			GrabBagModule.playerInstance.Visible = false;
// 			GrabBagModule.playerInstance.Collidable = false;
// 			playerFollower = GrabBagModule.playerInstance;
// 			BottomCenter = playerFollower.BottomCenter;

// 			base.Awake(scene);
// 		}

// 		public override void Update() {
// 			base.Update();

// 			CoyoteTime -= Engine.DeltaTime;

// 			if (Input.Jump.Pressed && CoyoteTime > 0) {
// 				Input.Jump.ConsumeBuffer();
// 				CoyoteTime = 0;
// 				Speed.Y = -4;
// 			}

// 			//Speed.X = Calc.Approach(Speed.X, Input.MoveX * 3, 8 * Engine.DeltaTime);
// 			Speed.Y = Calc.Approach(Speed.Y, functions.Float("max_fall", 4), functions.Float("grav") * Engine.DeltaTime);

// 			MoveH(Speed.X * Engine.DeltaTime * 60, CollideH);
// 			MoveV(Speed.Y * Engine.DeltaTime * 60, CollideV);

// 			playerFollower.Center = Center;
// 		}

// 		private int machineUpdate() {

// 			var function = functions[$"{stateNames[machine]}_update"] as LuaFunction;

// 			try {


// 				if (function != null) {

// 					int newState = -1;
// 					object[] array = function.Call();

// 					if (array != null && array.Length > 0) {

// 						if (array[0] is string) {
// 							for (int i = 0; i < stateNames.Length; ++i) {
// 								if (stateNames[i] == array[0] as string) {
// 									newState = i;
// 								}
// 							}
// 						}
// 						else
// 							newState = (int)array[0];
// 					}

// 					if (newState != -1)
// 						return newState;
// 				}

// 				if (updateErrorWait > 0)
// 					updateErrorWait++;
// 			}
// 			catch (Exception e) {
// 				if (updateErrorWait == 0)
// 					Logger.Log("Lua Character", $"Exception caught in custom player update \"{machine.State}\": {e}");

// 				updateErrorWait++;
// 			}

// 			if (updateErrorWait == 60)
// 				updateErrorWait = 0;

// 			return machine;
// 		}

// 		private void CollideH(CollisionData hitData) {
// 			Speed.X = 0;
// 		}
// 		private void CollideV(CollisionData hitData) {
// 			Speed.Y = 0;
// 			CoyoteTime = 0.1f;
// 		}

// 		public static void Walk(float max_speed, float accel) {

// 			Logger.Log("Lua Char", "Hello");
// 			instance.Speed.X = Calc.Approach(instance.Speed.X, Input.MoveX * max_speed, accel * Engine.DeltaTime);
// 		}

// 		public override void Render() {
// 			base.Render();

// 			Draw.Rect(Collider, Color.White);
// 		}
// 	}
// }
