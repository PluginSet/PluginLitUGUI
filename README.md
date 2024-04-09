* 使用`PackageObject`或`PackageSprites`来管理代码中直接引用的UI资源
* 使用`UIPackage`来获取资源、设置资源分支
* UI界面的Prefab可以挂载一个继承`PanelBehaviour`的脚本，用于管理UI界面的生命周期(非必须)
* 有`PanelBehaviour`的UI界面可以通过`PanelWindowBase`来定义界面入口，`PanelWindowBase`为代码添加，并用来管理UI的通用接口与生命周期
* UI界面需要通过`UIManager`进行注册，支持多次注册重载
* 可以使用`UILayer`来管理UI界面的层级