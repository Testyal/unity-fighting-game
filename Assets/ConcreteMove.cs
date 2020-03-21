using System;

class ConcreteMove : Move
{
    protected override Func<MovementController, MovementState> EnterStartup => (controller => 
        controller.State == MovementState.Jumping ? controller.EnterLanding() : MovementState.Disabled);
    
    protected override Func<MovementController, MovementState> EnterActive => (controller => controller.State);
    
    protected override Func<MovementController, MovementState> EnterRecovery => (controller => controller.State);

    protected override Func<MovementController, MovementState> EndMove => (controller =>
        controller.State == MovementState.Landing ? controller.EnterJumping() : MovementState.Stationary);
}