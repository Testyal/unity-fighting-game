using System;

class ConcreteMove : Move
{
    protected override Func<MovementController, MovementState> EnterStartup => (controller => controller.State);
    protected override Func<MovementController, MovementState> EnterActive => (controller => controller.State);
    protected override Func<MovementController, MovementState> EnterRecovery => (controller => controller.State);
}