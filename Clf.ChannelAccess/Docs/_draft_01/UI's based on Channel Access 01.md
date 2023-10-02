## UI's based on Channel Access

Notes on issues arising when a UI is accessing 'remote' data sources, eg 'remote' IOC's via Channel Access.

HISTORICAL, NEEDS UPDATING !!!

Kinds of information published by a PV :

Status information, inherently read-only. May be time varying. Cannot be directly influenced via the UI.
For example : numeric value of some measured quantity such as a position, or a true/false indication.
This is the easy one. You hope that what you're seeing displayed is up-to-date, but you're just a passive observer and the system isn't influenced by the integrity of the comms connection. 
If there are other observers as well as yourself, that doesn't affect anything.

At the other extreme, a 'write-only' value whose purpose is to trigger an action.
For example, there might be an integer-valued PV whose value doesn't really matter, but in order to trigger an action you write an update value that is one bigger than the integer value you've most recently been told about. When the PV responds with the incremented value, you assume that the command you issued has been actioned.
Lots of race conditions here, when there are several possible clients all of whom are able to issue a command. For example, even if you get back an incremented value, there's no way of knowing that it was *your* change that was actioned - it might have been another client that happened to increment that same value at roughly the same instant.

In between these extremes, consider the case of a PV that represents a 'desired-target-position' of a motorised actuator. On the server side, a motor is striving to reduce the discrepancy between that 'desired' position, as held in the sever's PV,  and an actual measured position. It does this by applying volts to a physical motor. Now on a client side UI, there might be a numeric value displayed which represents this 'desired' position. Suppose we provide a mechanism whereby a User can modify that Desired Position. That introduces some interesting complications and race conditions, which we'll consider in detail shortly. 
    
But before tackling that, consider the simpler case of a boolean valued PV that we're invited to change in our UI - for example, a PV that controls the speed at which a motor is meant to converge on a desired position, Fast or Slow.

In the UI, we'd show a check box labelled 'Fast convergence'. When we first connect, the check box would be showing the value acquired from the remote system - let's assume it's initially a 'false' value, indicating that the Fast Convergence mode is turned off.

The user can hover the mouse pointer over the check box, and see a tooltip that explains 'check this box to enable Fast Convergence'. If we click on the box, we'd expect the code behind the check box to submit a command to change to 'Fast Convergence' mode by chaging the boolean PV value from false to true, and for the check-box graphic to go into a 'checked' state ; but actually, if the UI were to immediately change the graphic to be checked, that would be fibbing. Really the UI should wait until it receives a confirmation message from the remote system, to tell us that our request has been honoured and that we're now in the desired 'Fast' mode. Until that message is received, the UI should be indicating 'state unknown', which for a check box is usually indicated by a dark grey fill.

In a sunny day scenario where everything is nominal, we'd expect only a momentary delay between clicking the check-box, seeing it go grey momentarily, and then seeing it show 'checked'.
However even in this seemingly trivial case, there are surprising edge cases that can (and will!) occur :
 - The remote system could successfully receive our request, but reject it for some reason, in which case the reply will tell us that the original 'false' value has not been altered. [ Hmm, with Channel Access this could be problematic, because unless the value has changed we don't get a notification. We'd have to rely on a timeout having ed with no 'loss-of-connection' message ].
 - We could have lost our connection to the remote system, so we genuinely no longer know the state. We'll get a notification of this loss, and will continue to show 'unknown'. If our policy for handling loss-of-connection is to display the last-reported-state, rather than unknown, 

!!! Pop-up EDIT !!!
Remote value continues to show ; updates when confirmed, popup dismissed.
Outcomes :
Confirmed ; rejected ; trumped-by-someone-else-writing-to-same-PV ; connection-lost
SHOULD SHOW EDITED VALUE IN A SEPARATE FIELD ??? !!!

So, now let's consider the more complicated case of letting a user type in a number that represents a new 'desired value' that we want to apply to a PV - such as the Position of an actuator.

In the UI, there will certainly be a display of the current numerical value, using something like an editable TextBox with a label :

    Desired position (mm)
    +----------------------+
    | 1.23                 |
    +----------------------+

The user is permitted to change this - how do we make that possible ?

One option might be to let her edit the value in the text box. As soon as editing commences we'd change the background colour (or similar) to indicate that the displayed value is no longer in sync with the actual 'remote' value. If the edited value would be inappropriate (eg blank, not-a-valid-number or a number that's outside the allowed range of values according to the upper and lower limits that we obtained from the PV) then we'd change the background to a different colour such as red. Then, once a plausible new value has been entered, we need to offer some way for the user to 'submit' that value to the PV. The common way to do this in desktop apps is to apply the edited value as soon as the 'focus' goes to a different UI element, via the user clicking on a different area of the screen or tabbing to a different control.

Unfortunately this scheme suffers from the same flaws that we outlined in the simpler 'check-box' example above.

Let's consider a mechanism that would accommodate those concerns ; it's a bit more complicated than what we've just described, but can neatly handle the edge cases.

[ AHA, WHAT WE'RE ABOUT TO DESCRIBE IS ACTUALLY WHAT HAPPENS IN TIM'S PHOEBUS UI !!! GOOD ... ]

The user clicks on the currently displayed value. As before, this is interpreted as a desire to enter an amended value and submit it to the remote system. However what happens next is that a 'value editing panel' drops down, underneath the displayed value ...

    Desired position (mm)
    +----------------------+
    | 1.23                 |  <== Displayed value continues to indicate the value
    +----------------------+      coming from the remote PV, which may be changing
    |                      |
    |         1.23         |  <== Value-editing Panel
    |                      |      Invites entry of an amended number,
    | |----|-------------| |      via typing it in or via a numeric keypad
    |                      |      or via dragging a slider etc ...
    +--------+----+--------+
    |        | OK | CANCEL |  <== Click to submit new value
    +--------+----+--------+

The panel can provide a choice of mechanisms for changing the value :
 - textual entry digit-by-digit
 - numeric keypad
 - slider (log or linear scale)
 - increment/decrement buttons
 - speech input !!
 - swipe-to-increment ?

The UI would provide an appropriate choice of input mechanisms for any given PV.

The whole while, the user can still see the 'current value' coming from the PV, which in principle could be changing.

When the user clicks OK (SUBMIT?) the panel stays in place (changing to show a 'progress' display?) until a response is received from the remote system. Hopefully this response will indicate that the submitted value has been accepted, in which case the Panel will collapse and we're done.
The other possibilities are
 - Comms was lost, so the new value wasn't applied.
 - The value was rejected, even though we'd submitted a value that was within the supposedly allowed limits. Hopefully we'll get to find out the reason why (??) and will be able to display an appropriate message. The panel will stay in place, and the user will be invited to have another attempt (or cancel).
 - Our value was accepted, but when the PV confirmed the updated value (via a Monitor event) the value that came back was not as expected. This could happen in the unlucky case that another client happened to submit a different value at the same time, and 'theirs' came just a bit later.
   In this case (??) we'd keep the panel open and invite our user to 'submit' again.

  ----------------------------

This scheme, where we avoid 'direct manipulation' of a displayed value and instead bring up some kind of  separate'editing' panel, might also be a better solution for simpler situations such as the check-box example mentioned above. The convention would be that you click something when you want to change it, and the 'live' remote value continues to be displayed while you configure the replacement value in a pop-up panel.

  -------------------------------

ATOMICITY ...

An IOC that's providing Image data would typically provide three separate PV's to represent the Height, the Width, and the image data bytes. That's unfortunate because PV update notifications come one-at-a-time, so if the IOC starts sending images of a different size, the client can get in a muddle. Version 4 'structs' address this problem ; a PV value can be a struct, that holds the two sizes plus the image data as an atomic item that's consistent.

CAN WE ACHIEVE THIS WITH V3 CHANNEL ACCESS USING 'FIELDS' ??
THE VALUE OF A PV COULD BE THE IMAGE DATA, AND THERE COULD BE FIELDS
 ON THAT PV WHICH WOULD REPRESENT THE HEIGHT AND WIDTH ???

-----------------------------

In any 'distributed system', you can never achieve 100% reliable synchronisation of values. This is the 'Two Generals' problem, and it's easy to prove that there is no solution. At best you can have 'eventual consistency'.

-----------------------

Every PV has a value, but PV's such as MotorRecord are 'special'.
VAL would get you a step count (integer) or a position (double) depending on what flavour of Motor you're talking to.

Is there a property on a PV that tells you it's a Motor Record ?
NO THERE ISN'T, A CLIENT THAT HAS CONNECTED TO A PV HAS TO 'KNOW' WHAT TYPE OF VALUE TO EXPECT ...












