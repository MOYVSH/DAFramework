namespace DAFramework
{
    public interface IModule
	{
		/// <summary>
		/// 创建模块
		/// </summary>
		void OnInit(System.Object createParam);

		/// <summary>
		/// 轮询模块
		/// </summary>
		void OnUpdate(float deltaTime, float unscaledDeltaTime);

		void OnLateUpdate();
		
		void OnFixedUpdate();

		/// <summary>
		/// 销毁模块
		/// </summary>
		void OnTermination();

	}
}