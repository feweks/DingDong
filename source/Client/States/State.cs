using Raylib_cs;

namespace Dong.Client.States;

class State
{
    public virtual void Create() { }
    public virtual void Update(float dt) { }
    public virtual void Render() { }
    public virtual void Destroy() { }
}